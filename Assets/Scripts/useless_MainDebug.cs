using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
    public Texture2D _referene;
    public RawImage _image;

    public ComputeShader _compute_diffImage;
    private int width = 1024;
    private int height = 1024;

    //Debug 
    int[,,] _grid;

    void Start()
    {

        Brush brush             = new Brush(2.0f,8.0f,1.0f);  

        /*--------------------------------------------------------------------------------------------------------
        * Example add shot on masterpiece
        -------------------------------------------------------------------------------------------------------*/
        //Masterpiece masterpieceDebug = new Masterpiece(width, height, brush);
        //masterpieceDebug.addShot( 0 , 1 , new Vector2(width/2.0f, height/2.0f), 100, new Vector2(0.5f,0.5f), Color.black);
        //_image.texture          = masterpieceDebug.renderPiece()._texture;
        //return;

        /*-------------------------------------------------------------------------------------------------------
        * Example paint random masterPiece
        -------------------------------------------------------------------------------------------------------*/
        //System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
        //timer.Start();
        //
        ////Masterpiece masterpiece = new Masterpiece(width, height, brush);    
        //Masterpiece masterpiece = Masterpiece.randomPaint(width, height, brush, 1000);  
        //
        //timer.Stop();
        //Debug.Log("random paint : "+timer.ElapsedMilliseconds+"ms");
        //timer.Restart();
        //
        //_image.texture          = masterpiece.renderPiece()._texture;
        //
        //timer.Stop();
        //Debug.Log("render texture : "+timer.ElapsedMilliseconds+"ms");

        /*-------------------------------------------------------------------------------------------------------
        * Test de objets AG
        -------------------------------------------------------------------------------------------------------*/
//        Canvas canvas = new Canvas(width, height);
//        BrushShot shot = new BrushShot(brush, 0 , 1 , new Vector2(width/2.0f, height/2.0f), 100, new Vector2(0.5f,0.5f), Color.black );
//        AG_gene gene = new AG_gene(shot);
//        List<AG_gene> genes = new List<AG_gene>();
//        for(int i = 0;i<1000;i++){ genes.Add(gene);}
//        AG_adn adn = new AG_adn(genes.ToArray());
//        System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
//        Color[] c = _referene.GetPixels();
//        AG_adn_default_values.init(width, height);
//        timer.Start();
//        //Debug.Log(adn.ScoreGPU(c, brush, canvas, _compute_diffImage, true));
//        Debug.Log(adn.Score(c, brush, canvas, true));
//        timer.Stop();
//        Debug.Log("Score : "+timer.ElapsedMilliseconds+"ms");
//        timer.Restart();
//
//        _image.texture = adn.Render(canvas, brush);    
//        timer.Stop();


//        Debug.Log("Render : "+timer.ElapsedMilliseconds+"ms");

        /*-------------------------------------------------------------------------------------------------------
        * Example on training model
        -------------------------------------------------------------------------------------------------------*/
        //AG_adn_default_values.init(width, height);
        //Canvas canvas = new Canvas(width, height);
        //AG_trainer trainer = new AG_trainer(_referene.GetPixels(), canvas, brush, 100, 100);        
        //trainer.RunTraining(10000, 
        //    (adn, iteration, pool)=>{
        //        ThreadDispatch.Instance().Enqueue(()=>{
        //            Debug.Log(adn._score);
        //            _image.texture = adn.Render(canvas, brush);                
        //        });
        //    }, 
        //    (pool, adn)=>{
        //        ThreadDispatch.Instance().Enqueue(()=>{
        //            _image.texture = adn.Render(canvas, brush);                
        //        });
        //        Debug.Log("done");
        //    }
        //);

        /*-------------------------------------------------------------------------------------------------------
        * Filtre de convolution
        --------------------------------------------------------------------------------------------------------*/
        /*
        -1  0  1
        -2  0  2
        -1  0  1
        */

//       float[][] kernel_h = new[]{
//           new[]{ -1f , -2f , -1f },
//           new[]{  0f ,  0f ,  0f },
//           new[]{  1f ,  2f ,  1f }
//       };
//
//       float[][] kernel_v = new[]{
//           new[]{ -1f ,  0f , 1f },
//           new[]{ -2f ,  0f , 2f },
//           new[]{ -1f ,  0f , 1f }
//       };
//
//       float[][] kernel_p = new[]{
//           new[]{ -1f ,  0f , 1f },
//           new[]{ -1f ,  0f , 1f },
//           new[]{ -1f ,  0f , 1f }
//       };
//
//       
//       float[][] kernel_g = new float[32][];
//       for(int i = 0;i<kernel_g.Length;i++){
//           kernel_g[i] = new float[kernel_g.Length];
//           for(int j = 0 ; j < kernel_g.Length ; j++){
//               kernel_g[i][j] = 1.0f/(kernel_g.Length*kernel_g.Length);
//           }
//       }
//
//       Convolution conv = new Convolution(_referene.GetPixels(), width, height, kernel_p );
//       conv.grayScale();
//       Color[] stensil = conv.apply(); 

        //Texture2D tex = new Texture2D(width, height);
        //tex.SetPixels(stensil);
        //tex.Apply();
        //_image.texture = tex;  
        
        /*-------------------------------------------------------------------------------------------------------
        * Exemple de peinture naive
        -------------------------------------------------------------------------------------------------------*/
        NaivePainter naif = new NaivePainter(_referene.GetPixels(),null, width, height, brush, 80, null);
        Texture2D tex = new Texture2D(width, height);

/*EN MODE THREADED LIVE*/        
//        System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
//        timer.Start();
//
//        naif.runNaiveThread( 
//
//            colors=>{
//
//                ThreadDispatch.Instance().Enqueue(()=>{
//                    
//                    if(timer.ElapsedMilliseconds > 500){
//                        tex.SetPixels( colors);
//                        tex.Apply();
//                        _image.texture = tex;  
//                        timer.Restart();
//                    } 
//                   
//
//                });
//            },
//            finalNaif=>{
//                Debug.Log("done");
//
//            }
//        );

/*EXECUTION SANS THREAD*/        

        System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
        timer.Start();
        
        tex.SetPixels(naif.runNaive(5000));
        timer.Stop();

        Debug.Log(timer.ElapsedMilliseconds+"ms");

        tex.Apply();
        _image.texture = tex;  


        /*-------------------------------------------------------------------------------------------------------
        * Exemple d'extraction des couleurs
        -------------------------------------------------------------------------------------------------------*/
//        color_extracter extracter = new color_extracter();
//        extracter.extract(_referene.GetPixels(), 8, Grid=>{  _grid = Grid; });
//        Color[] palette = extracter._palette;
//
//        GameObject canvas = GameObject.Find("Canvas");
//
//        float step = 1.0f/palette.Length;
//
//        for( int i = 0 ; i < palette.Length ; i++ ){
//
//            GameObject go_color = new GameObject("c_"+i , typeof(RectTransform));
//            go_color.transform.parent = canvas.transform;
//
//            RectTransform rt = go_color.GetComponent<RectTransform>();
//            rt.anchorMin = new Vector2(i*step,0);
//            rt.anchorMax = new Vector2((i+1.0f)*step,1.0f/10.0f);
//            rt.offsetMin = new Vector2(0,0);                                      
//            rt.offsetMax = new Vector2(0,0);  
//            
//            Image img = go_color.AddComponent<Image>();
//            img.color = palette[i];
//        }


    }

    void OnDrawGizmosSelected()
    {

        if(_grid == null){
            return;
        }
            for(int i=0;i<255;i++){
            for(int j=0;j<255;j++){
                for(int k=0;k<255;k++){ 
                    
                    if(_grid[i,j,k] > 0){


                        Gizmos.color = new Color(i/255.0f,j/255.0f,k/255.0f);
                        Gizmos.DrawSphere(new Vector3(i,j,k), 1);
                        
                    }
                }
            }
        }
    }
    
    void Update()
    {
        
    }
}
