using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Crosstales.FB;
using UnityEngine;
using UnityEngine.UI;

public class Procedural_painting : MonoBehaviour
{
    /**************************************************
    *                   UI
    **************************************************/
    public RawImage raw_image;
    public GameObject div_palette;

    public Text text_brushMinValue;
    public Text text_brushMaxValue;
    public Text text_brushStrokeNumber;
    public Text text_brushMaxLength;
    public Text text_nbOfColors;
    
    public Slider slider_brushMinValue;
    public Slider slider_brushMaxValue;
    public Slider slider_brushStrokeNumber;
    public Slider slider_brushMaxLength;
    public InputField input_canvasWidth;
    public InputField input_canvasHeight;
    public Toggle toggle_increase_outline_resolution;
    public Slider slider_nb_colors;

    public Button btn_importImage;
    public Button btn_removeImage;
    public Button btn_paint;

    /**************************************************
    *                   Runtime
    **************************************************/
    private Texture2D intput_image;
    private Texture2D intput_processed_image;
    private Texture2D output_image;
    private Color[] palette;
    
    /**************************************************
    *                   Global variable
    **************************************************/
    private int reference_resolution_X = 720;
    private int reference_resolution_Y = 720;

    private float stensil_threshold = 0.6f;

    private float[][] kernel_p = new[]{
        new[]{ -1f , -2f , -1f },
        new[]{  0f ,  0f ,  0f },
        new[]{  1f ,  2f ,  1f }
    };


    void Start()
    {
        slider_brushMinValue.onValueChanged.AddListener(value=>{AValueChanged();});   
        slider_brushMaxValue.onValueChanged.AddListener(value=>{AValueChanged();});   
        slider_brushStrokeNumber.onValueChanged.AddListener(value=>{AValueChanged();});   
        slider_brushMaxLength.onValueChanged.AddListener(value=>{AValueChanged();});  
        slider_nb_colors.onValueChanged.AddListener(value=>{AValueChanged();extractColors();});  

        input_canvasWidth.onValueChanged.AddListener(value=>{AValueChanged();processInputImage();});
        input_canvasHeight.onValueChanged.AddListener(value=>{AValueChanged();processInputImage();});

        toggle_increase_outline_resolution.onValueChanged.AddListener(value=>{AValueChanged();});

        btn_importImage.onClick.AddListener(ImportImage);
        btn_removeImage.onClick.AddListener(RemoveImage);
        btn_paint.onClick.AddListener(()=>{StartCoroutine(PaintNow());});

        AValueChanged();
    }


   /*************************************************************************************************************
    * Affichage de la texture sur l'algo est passé
    *************************************************************************************************************/
    private void DisplayOutput(){

        raw_image.texture = output_image;
        
        AspectRatioFitter ratioFitter = raw_image.gameObject.GetComponent<AspectRatioFitter>();
        if(ratioFitter != null){
            
            ratioFitter.aspectRatio = (float)output_image.width/(float)output_image.height;

        }

    }

    /*************************************************************************************************************
    * Affichage de l'image importée
    *************************************************************************************************************/
    private void DisplayInput(){

        raw_image.texture = intput_image;
        
        AspectRatioFitter ratioFitter = raw_image.gameObject.GetComponent<AspectRatioFitter>();
        if(ratioFitter != null){


            ratioFitter.aspectRatio = (float)intput_image.width/(float)intput_image.height;

        }

    }

    /*************************************************************************************************************
    * Impport d'une image + calcul la t2D sur laquelle travailler
    *************************************************************************************************************/
    private void ImportImage(){
        
        string path = FileBrowser.OpenSingleFile("Open single file", ".", new ExtensionFilter("Image Files", "png", "jpg", "jpeg"));
        
        if (File.Exists(path))     {

            byte[] fileData = File.ReadAllBytes(path);

            intput_image = null;
        
            intput_image = new Texture2D(2, 2);
            intput_image.LoadImage(fileData); 

            processInputImage();

            //File.WriteAllBytes("/Users/gael/Downloads/crop2.png" + ".png", intput_processed_image.EncodeToPNG());
            DisplayInput();

            extractColors();

        }

        AValueChanged();

    }

    /*************************************************************************************************************
    * Supprime l'image actuellement en traitement
    *************************************************************************************************************/
    private void RemoveImage(){

        intput_image           = Texture2D.whiteTexture;
        intput_processed_image = Texture2D.whiteTexture;
        output_image           = Texture2D.whiteTexture;
        raw_image.texture = intput_image;

        clearPalette();
        AValueChanged();

    }

    /*************************************************************************************************************
    * Event quand un paramettre est modifié
    *************************************************************************************************************/
    private void AValueChanged(){

        text_brushMinValue.text = "Brush min size : "+slider_brushMinValue.value+"mm";
        text_brushMaxValue.text = "Brush max size : "+slider_brushMaxValue.value+"mm";
        text_brushStrokeNumber.text = "Brush stroke number : "+slider_brushStrokeNumber.value;
        text_brushMaxLength.text = "Brush max length : "+slider_brushMaxLength.value+"mm";
        text_nbOfColors.text = "Number of colors : "+slider_nb_colors.value;

        if( !string.IsNullOrEmpty(input_canvasWidth.text)  && !string.IsNullOrEmpty(input_canvasHeight.text)  
            && Int32.TryParse(input_canvasWidth.text, out int canvas_width) && Int32.TryParse(input_canvasHeight.text, out int canvas_height) ){

            btn_importImage.interactable = true;


            if( intput_image != null && intput_image != Texture2D.whiteTexture  ){

                btn_importImage.gameObject.SetActive(false);
                btn_removeImage.gameObject.SetActive(true);

                btn_paint.interactable = true;
                
            }else{

                btn_paint.interactable = false;
                btn_importImage.gameObject.SetActive(true);
                btn_removeImage.gameObject.SetActive(false);

            }

        }else{
            
            btn_importImage.interactable = false;

        }



    }

    /*************************************************************************************************************
    * Run painting algorithm
    *************************************************************************************************************/
    private IEnumerator PaintNow(){


        if(intput_processed_image != null){

            btn_paint.GetComponentInChildren<Text>().text = "Painting ...";
            btn_paint.interactable = false;
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            Brush brush           = new Brush( brushMMToPx( slider_brushMinValue.value/2.0f ) , brushMMToPx( slider_brushMaxValue.value/2.0f ),1.0f);  
            NaivePainter naif     = new NaivePainter(intput_processed_image.GetPixels(),null, intput_processed_image.width, intput_processed_image.height, brush, (int)brushMMToPx( slider_brushMaxLength.value),  palette);

            Color[] outputColors  = naif.runNaive( (int)slider_brushStrokeNumber.value );

            if(toggle_increase_outline_resolution.isOn){
                
                Convolution conv = new Convolution(intput_processed_image.GetPixels(), intput_processed_image.width, intput_processed_image.height, kernel_p );
                conv.grayScale();
                Color[] stensil = conv.apply(stensil_threshold); 
                
                brush = new Brush( brushMMToPx( slider_brushMinValue.value/2.0f ) , brushMMToPx( slider_brushMinValue.value/2.0f ),1.0f);  
                naif  = new NaivePainter(intput_processed_image.GetPixels(),stensil, intput_processed_image.width, intput_processed_image.height, brush, (int)brushMMToPx( slider_brushMaxLength.value/2.0f),  palette);
                Color[] outputColorsWithStensil  = naif.runNaive( (int)slider_brushStrokeNumber.value );

                for(int i  =0; i < outputColorsWithStensil.Length;i++ ){
                    outputColors[i] = outputColorsWithStensil[i] == Color.white ? outputColors[i] : outputColorsWithStensil[i];
                }
            }

            if(output_image != null && (output_image.width != intput_processed_image.width || output_image.height != intput_processed_image.height) ){
                output_image = null;
            }

            if(output_image == null ){
                output_image = new Texture2D(intput_processed_image.width, intput_processed_image.height);
            }

            output_image.SetPixels( outputColors );
            output_image.Apply();
            DisplayOutput();
            
            btn_paint.GetComponentInChildren<Text>().text = "Paint";


        }

    }


    /*************************************************************************************************************
    * Permet juste d'etre appeller sur different event sans avoir besoin de retrouver tous les paramettres
    *************************************************************************************************************/
    public void processInputImage(){


        if( string.IsNullOrEmpty(input_canvasWidth.text)  || string.IsNullOrEmpty(input_canvasHeight.text)  
            || !Int32.TryParse(input_canvasWidth.text, out int canvas_width) || !Int32.TryParse(input_canvasHeight.text, out int canvas_height )
            || intput_image == null  ){

                return;
        }

        Debug.Log("reprocess image");

        intput_processed_image = processImage(
            intput_image, Int32.Parse(input_canvasWidth.text), 
            Int32.Parse(input_canvasHeight.text), 
            reference_resolution_X, reference_resolution_Y);

    }

    /*************************************************************************************************************
    * Reduit la résolution de l'image pour avoir reduire la quantitée de donnée a traiter, crop également
    * l'image pour venir fitter au ratio de la toile
    *************************************************************************************************************/
    private Texture2D processImage(Texture2D source, int largeurMM, int hauteurMM, int maxWidthPX, int maxHeightPX){
        
        //Remet l'image dans une résolution constante pour ne pas avoir trop de data
        Texture2D texture_resized = resizeImage(source, maxWidthPX, maxHeightPX);

        int width  = texture_resized.width;
        int height = texture_resized.height;


        Debug.Log("size depart : "+width+"x"+height);

        //Etirement de la largeur, voir s'il la hauteur peut enssuite etre coupée pour passer dans le format
        float coef = (float)largeurMM/(float)width;
        if(height * coef > hauteurMM){
            
            Debug.Log("conserve la largeur");
            float coefH = (float)hauteurMM/(float)largeurMM;
            Debug.Log("size final : "+width+"x"+(int)(width * coefH)+" : "+coefH);

            return ResampleAndCrop(texture_resized,  width,  (int)(width * coefH));      
            
        }else{

            //Etirement de la hauteur, voir s'il la largeur peut enssuite etre coupée pour passer dans le format

            coef = (float)hauteurMM/(float)height;
            if(width * coef > largeurMM){

                Debug.Log("conserve la hauteur");
                float coefW = (float)largeurMM/(float)hauteurMM;
                Debug.Log("size final : "+(int)(height * coefW)+"x"+height+" : "+coefW );

                return ResampleAndCrop(texture_resized, (int)(height * coefW), height);

            }else{

                Debug.Log("pas de solutions");
                return texture_resized;
            }

        }

    }

    
    /*************************************************************************************************************
    * Crop l'image 
    *************************************************************************************************************/
    public Texture2D ResampleAndCrop(Texture2D source, int targetWidth, int targetHeight)
    {
        int sourceWidth = source.width;
        int sourceHeight = source.height;
        float sourceAspect = (float)sourceWidth / sourceHeight;
        float targetAspect = (float)targetWidth / targetHeight;
        int xOffset = 0;
        int yOffset = 0;
        float factor = 1;
        if (sourceAspect > targetAspect)
        { // crop width
            factor = (float)targetHeight / sourceHeight;
            xOffset = (int)((sourceWidth - sourceHeight * targetAspect) * 0.5f);
        }
        else
        { // crop height
            factor = (float)targetWidth / sourceWidth;
            yOffset = (int)((sourceHeight - sourceWidth / targetAspect) * 0.5f);
        }
        Color32[] data = source.GetPixels32();
        Color32[] data2 = new Color32[targetWidth * targetHeight];
        for (int y = 0; y < targetHeight; y++)
        {
            for (int x = 0; x < targetWidth; x++)
            {
                var p = new Vector2(Mathf.Clamp(xOffset + x / factor, 0, sourceWidth - 1), Mathf.Clamp(yOffset + y / factor, 0, sourceHeight - 1));
                // bilinear filtering
                var c11 = data[Mathf.FloorToInt(p.x) + sourceWidth * (Mathf.FloorToInt(p.y))];
                var c12 = data[Mathf.FloorToInt(p.x) + sourceWidth * (Mathf.CeilToInt(p.y))];
                var c21 = data[Mathf.CeilToInt(p.x) + sourceWidth * (Mathf.FloorToInt(p.y))];
                var c22 = data[Mathf.CeilToInt(p.x) + sourceWidth * (Mathf.CeilToInt(p.y))];
                var f = new Vector2(Mathf.Repeat(p.x, 1f), Mathf.Repeat(p.y, 1f));
                data2[x + y * targetWidth] = Color.Lerp(Color.Lerp(c11, c12, p.y), Color.Lerp(c21, c22, p.y), p.x);
            }
        }

        var tex = new Texture2D(targetWidth, targetHeight);
        tex.SetPixels32(data2);
        tex.Apply(true);
        return tex;
    }
    
    /*************************************************************************************************************
    * modifie a résolution de l'image
    *************************************************************************************************************/
    private Texture2D resizeImage(Texture2D source, int maxWidth, int maxHeigth){

        int targetWidth;
        int targetHeight;

        if(source.width > source.height && source.width > maxWidth){

            targetWidth = maxWidth;
            targetHeight = (int)((float)maxWidth * ((float)source.height/(float)source.width));
            //Debug.Log("convert to "+targetWidth+" x "+targetHeight);


        }else if(source.width < source.height && source.height > maxHeigth){
            
            targetHeight = maxHeigth;
            targetWidth = (int)((float)maxHeigth * ((float)source.width/(float)source.height));
            //Debug.Log("convert to "+targetWidth+" x "+targetHeight);

        }else{
            //Debug.Log("not resized "+source.width+" x "+source.height);
            return source;
        }

        Texture2D result=new Texture2D(targetWidth,targetHeight,source.format,true);
        Color[] rpixels=result.GetPixels(0);
        float incX=(1.0f / (float)targetWidth);
        float incY=(1.0f / (float)targetHeight); 
        for(int px=0; px<rpixels.Length; px++) { 
            rpixels[px] = source.GetPixelBilinear(incX*((float)px%targetWidth), incY*((float)Mathf.Floor(px/targetWidth))); 
        } 
        result.SetPixels(rpixels,0); 
        result.Apply(); 
        return result; 

    }

    /*************************************************************************************************************
    * Convertion d'une taille en mm vers une taille en px
    *************************************************************************************************************/
    private float brushMMToPx(float brushMM){


        float width_mm = Int32.Parse(input_canvasWidth.text);
        float width_px = intput_processed_image.width;
    
        return (brushMM * width_px) / width_mm;
    }


    /*************************************************************************************************************
    * Extraction d'une palette de couleur, seul ces couleurs seront enssuite utilisés pour peindre
    *************************************************************************************************************/
    private void extractColors(){

        if(intput_image == null || intput_image == Texture2D.whiteTexture){
            return;
        }

        Texture2D very_low_res = resizeImage(intput_image, 12,12);

        color_extracterV2 extracter  =  new color_extracterV2(very_low_res.GetPixels(), (int)slider_nb_colors.value);
        extracter.extract();
        palette = extracter._output_colors;

    
        GameObject canvas = GameObject.Find("Canvas");

        clearPalette();

        float step = 1.0f/palette.Length;

        for( int i = 0 ; i < palette.Length ; i++ ){

            GameObject go_color = new GameObject("c_"+i , typeof(RectTransform));
            go_color.transform.parent = div_palette.transform;

            RectTransform rt = go_color.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(i*step,0);
            rt.anchorMax = new Vector2((i+1.0f)*step,1.0f);
            rt.offsetMin = new Vector2(0,0);                                      
            rt.offsetMax = new Vector2(0,0);  
            
            Image img = go_color.AddComponent<Image>();
            img.color = palette[i];

            Button btn = go_color.AddComponent<Button>();
            int finalIndex = i;
            Color openColor = palette[finalIndex];
            btn.onClick.AddListener(()=>{
                
                ColorPicker.Create( openColor , "Choose the cube's color!", onColorChoosing=>{

                    img.color = onColorChoosing;
                    AValueChanged();

                } , onColorDone=>{

                    img.color = onColorDone;
                    palette[finalIndex] = onColorDone;
                    AValueChanged();

                }, true);

            });
        }

    }

    private void clearPalette(){
        foreach (Transform child in div_palette.transform) {
            Destroy(child.gameObject);
        }

    }



}
