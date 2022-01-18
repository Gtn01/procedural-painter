using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class color_extracter 
{

   //resultat des couleurs extraites 
   public Color[] _palette;
   //dictionaire de conversion ancienne couleur vers palette
   public Dictionary<Color, Color> _color_ref;



    public color_extracter(){

        _palette = new Color[0];
        _color_ref = new Dictionary<Color, Color>();

    }

   /*************************************************************************************
   *
   * Extrait une palette de NbColor du tableau de couleur
   *
   *************************************************************************************/
   public void extract(Color[] colors, int nbColors, Action<int[,,]> callbackDebug ){

        _color_ref = new Dictionary<Color, Color>();

        int[,,] color_grid = new int[255,255,255];
        List<Vector3Int> filledColors = new List<Vector3Int>(); 

        for(int i=0;i<255;i++){
            for(int j=0;j<255;j++){
                for(int k=0;k<255;k++){ 
                    color_grid[i,j,k] = 0;
                }
            }
        }

        int x;
        int y;
        int z;

        for(int i = 0 ; i < colors.Length;i++ ){

            x = (int)(colors[i].r*254);
            y = (int)(colors[i].g*254);
            z = (int)(colors[i].b*254);
            color_grid[x,y,z] ++;

            filledColors.Add(new Vector3Int(x,y,z));
        }

        /*Profondeur dans l'arbre pour avoir au moins nbColors différentes*/
        int interations = (int)Mathf.Ceil(( Mathf.Log(nbColors)/Mathf.Log(2) ) );
        List<Vector3Int[]> running_colors = new List<Vector3Int[]>();
        List<int[,,]> running_grids       = new List<int[,,]>();

        running_colors.Add(filledColors.ToArray());
        running_grids.Add(color_grid);


        for(int i = 0 ; i< interations;i++){
            
            List<Vector3Int[]> next_running_colors = new List<Vector3Int[]>();
            List<int[,,]> next_running_grids       = new List<int[,,]>();

            for(int j = 0;j<running_grids.Count;j++){

                medianSplit(running_colors[j], running_grids[j], out Vector3Int[] filledColors_left, out int[,,] gridColor_left, out Vector3Int[] filledColors_right, out int[,,] gridColor_right );
                next_running_colors.Add(filledColors_left);   
                next_running_colors.Add(filledColors_right);   
                next_running_grids.Add(gridColor_left);
                next_running_grids.Add(gridColor_right);
            }

            running_colors = next_running_colors;
            running_grids = next_running_grids;

        }


        /*Reconversion sous forme de couleurs*/
        List<Color> overfilled_pallette = new List<Color>();

        for(int i = 0;i<running_colors.Count;i++){
            
            overfilled_pallette.Add(getColor(running_colors[i], running_grids[i]));
        }

        //TODO continuer en faisant merger les couleurs les plus proches pour avoir le compte de couleurs
        _palette = overfilled_pallette.ToArray();
        Debug.Log("Extracrtion done : "+running_grids.Count+" colors found ! ");

   }

   /*************************************************************************************
   *
   * Decoupe une grille de couleur 3D en deux pour avoir la meme desnité des deux cotés
   *
   *************************************************************************************/
   public void medianSplit(Vector3Int[] filledColors, int[,,] grid, out Vector3Int[] filledColors_left, out int[,,] gridColor_left, out Vector3Int[] filledColors_right, out int[,,] gridColor_right, int deep = -1){


       int gridSize         = 255;
       float best_scrore    = int.MaxValue;
       int best_pos         = -1;
       Vector3Int best_axis = new Vector3Int(0,0,0);
       float N                = 16; //Pas d'itération dans la grille de couleurs

       Vector3Int Axe_X = new Vector3Int(1,0,0);
       Vector3Int Axe_Y = new Vector3Int(0,1,0);
       Vector3Int Axe_Z = new Vector3Int(0,0,1);

       filledColors_left  = null;
       gridColor_left     = null;
       filledColors_right = null;
       gridColor_right    = null;

       //TODO relancer l'opération jusqu'a avoir le bon compte de couleur dans la palette

       //Parcours tous les endroits ou couper la grille
       for( int i = 0 ; i < N ; i++ ){ 

           int cutPos = (int)(( ( 1.0f/N ) * i ) * gridSize);
           float scoreX = splitScore(filledColors, grid, Axe_X, cutPos);
           float scoreY = splitScore(filledColors, grid, Axe_Y, cutPos);
           float scoreZ = splitScore(filledColors, grid, Axe_Z, cutPos);

            if( best_scrore >  scoreX ){
                best_scrore = scoreX;
                best_pos    = cutPos;
                best_axis   = Axe_X; 
            }

            if( best_scrore >  scoreY ){
                best_scrore = scoreY;
                best_pos = cutPos;
                best_axis   = Axe_Y; 
            }

            if( best_scrore >  scoreZ ){
                best_scrore = scoreZ;
                best_pos = cutPos;
                best_axis   = Axe_Z; 
            }

       }


       splitGrid( filledColors, grid,  best_axis,  best_pos,  out filledColors_left,  out gridColor_left,  out filledColors_right,  out gridColor_right );
   }


    /*************************************************************************************
   *
   * Retourne le difference entre les deux partitions, plus la partition est petit
   * plus la position de coupe est bonne car les deux partition on la meme densité
   *
   *************************************************************************************/
   public float splitScore(Vector3Int[] filledColors, int[,,] grid, Vector3Int axis, int position){

       int major = 0;
       int minor = 0;

        bool[] emptySlice = Enumerable.Repeat(true, 255).ToArray();

       if(axis.x == 1){

            //Parcours toutes les cases remplies de la grile        
            for( int i = 0 ; i < filledColors.Length ; i++ ){
                
                Vector3Int colorPos = filledColors[i];
                emptySlice[colorPos.x] = false;

                if( colorPos.x <= position ){

                    major += grid[colorPos.x , colorPos.y , colorPos.z];

                }else{
                    
                    minor += grid[colorPos.x , colorPos.y , colorPos.z];
                }
            }

       }else if(axis.y == 1){

            //Parcours toutes les cases remplies de la grile        
            for( int i = 0 ; i < filledColors.Length ; i++ ){
                
                Vector3Int colorPos = filledColors[i];
                emptySlice[colorPos.y] = false;

                if( colorPos.y <= position ){
                    
                    major += grid[colorPos.x , colorPos.y , colorPos.z];

                }else{
                    
                    minor += grid[colorPos.x , colorPos.y , colorPos.z];
                }
            }

       }else if(axis.z == 1){

            //Parcours toutes les cases remplies de la grile        
            for( int i = 0 ; i < filledColors.Length ; i++ ){
                
                Vector3Int colorPos = filledColors[i];
                emptySlice[colorPos.z] = false;

                if( colorPos.z <= position ){
                    
                    major += grid[colorPos.x , colorPos.y , colorPos.z];

                }else{
                    
                    minor += grid[colorPos.x , colorPos.y , colorPos.z];
                }
            }

       }

       int count_emptySlices = emptySlice.Where(x => x.Equals(true)).Count();
       //Debug.Log("[d:"+deep+" | p:"+position+" ]  ( "+major +" - "+ minor+" )" +" - "+(count_emptySlices*255*255)+" = "+ (Math.Abs( major - minor ) - (count_emptySlices*255*255)) );

       return Math.Abs( major - minor ) - (count_emptySlices*255*255);
   }
   

   /*************************************************************************************
   *
   * Decoupe une grille et retourne deux nouvelle grille sous forme de deux nouveau objets
   *
   *************************************************************************************/
   public void splitGrid(Vector3Int[] filledColors, int[,,] grid, Vector3Int axis, int position, out Vector3Int[] filledColors_left, out int[,,] gridColor_left, out Vector3Int[] filledColors_right, out int[,,] gridColor_right){

       //Grille de couleur complette vide
       gridColor_left  = new int[255,255,255];
       gridColor_right = new int[255,255,255];

       for(int i=0;i<255;i++){
           for(int j=0;j<255;j++){
               for(int k=0;k<255;k++){ 

                   gridColor_left[i,j,k]  = 0;
                   gridColor_right[i,j,k] = 0;
               }
           }
       }


       List<Vector3Int> list_filledColors_left  = new List<Vector3Int>();
       List<Vector3Int> list_filledColors_right = new List<Vector3Int>();

       if(axis.x == 1){

            for( int i = 0 ; i < filledColors.Length ; i++ ){

                Vector3Int colorPos = filledColors[i];

                if( colorPos.x <= position ){
                    list_filledColors_left.Add( new Vector3Int(colorPos.x , colorPos.y , colorPos.z) );
                    gridColor_left[colorPos.x , colorPos.y , colorPos.z] ++;
                    
                }else{
                    list_filledColors_right.Add( new Vector3Int( colorPos.x , colorPos.y , colorPos.z) );
                    gridColor_right[colorPos.x , colorPos.y , colorPos.z] ++;

                }
            }

       }else if(axis.y == 1){

            for( int i = 0 ; i < filledColors.Length ; i++ ){

                Vector3Int colorPos = filledColors[i];

                if( colorPos.y <= position ){
                    list_filledColors_left.Add( new Vector3Int(colorPos.x , colorPos.y , colorPos.z) );
                    gridColor_left[colorPos.x , colorPos.y , colorPos.z] ++;
                    
                }else{
                    list_filledColors_right.Add( new Vector3Int( colorPos.x , colorPos.y , colorPos.z) );
                    gridColor_right[colorPos.x , colorPos.y , colorPos.z] ++;

                }
            }
           
       }else if(axis.z == 1){
            
            for( int i = 0 ; i < filledColors.Length ; i++ ){

                Vector3Int colorPos = filledColors[i];

                if( colorPos.z <= position ){
                    list_filledColors_left.Add( new Vector3Int(colorPos.x , colorPos.y , colorPos.z) );
                    gridColor_left[colorPos.x , colorPos.y , colorPos.z] ++;
                    
                }else{
                    list_filledColors_right.Add( new Vector3Int( colorPos.x , colorPos.y , colorPos.z) );
                    gridColor_right[colorPos.x , colorPos.y , colorPos.z] ++;

                }

            }

       }


       //Retourne la liste des cases remplit dans la grille de dcouleurs
       filledColors_left  = list_filledColors_left.ToArray();    
       filledColors_right = list_filledColors_right.ToArray();      
       
   }

   /*************************************************************************************
   *
   * Récupération d'une couleur en fonction de la grille et d'une liste de couleur
   *
   *************************************************************************************/
   public Color getColor(Vector3Int[] filledColors, int[,,] grid){

        float r = 0;
        float g = 0;
        float b = 0;
        Vector3Int pos;
        float c = 0;
        float coef;

        
        for( int i = 0 ; i < filledColors.Length ; i++ ){
            
            pos = filledColors[i];
            coef = grid[pos.x, pos.y, pos.z];
            c += coef;
            r += pos.x * coef;
            g += pos.y * coef;
            b += pos.z * coef;
        }

        if(c == 0){
            return Color.black;
        }

        return new Color( (r/c)/255.0f , (g/c)/255.0f , (b/c)/255.0f );

   }


}