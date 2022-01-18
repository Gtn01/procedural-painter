using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Canvas 
{
    public int _width;
    public int _height;
    public Texture2D _texture;

    public Canvas(int width, int height){

        _width = width;
        _height = height;
        _texture = new Texture2D(width, height, textureFormat:TextureFormat.ARGB32 , mipCount:3 , linear:true);
        _texture.filterMode = FilterMode.Point;// no smooth pixels
        
    }
    
    /*************************************************************************************
    * Calcul les pixel a colorer pour afficher un coup de pinceau sur la texture
    *************************************************************************************/
    public void addShot(BrushShot shot){
        
        Vector2 dir  = shot._direction.normalized;
        Vector2 pos  = shot._position;
        float length = shot._length;
        
        for( int i = 0 ; i < length ; i++ ){

            pos = shot._position + i*dir;
            fillCircle( (int)pos.x, (int)pos.y, (int)shot.sizeAt(i) , shot._color);

        }

        _texture.Apply();

    }

    public void addShots(BrushShot[] shots){

        Color[] colors   = new Color[_width*_height];
        for(int i = 0;i<colors.Length;i++){
            colors[i] = Color.white;
        }

        BrushShot shot;
        Vector2 dir;
        Vector2 pos;
        float length;

        for(int i = 0;i<shots.Length;i++){

            shot = shots[i];
            dir  = shot._direction.normalized;
            pos  = shot._position;
            length = shot._length;

            for( int j = 0 ; j < length ; j++ ){

                pos = shot._position + j * dir;
                fillCircle(ref colors, _width, (int)pos.x, (int)pos.y, (int)shot.sizeAt(j) , shot._color);

            }

        }

        _texture.SetPixels(colors);
        _texture.Apply();


    }


    /*************************************************************************************
    * Affichage d'un cercle vide
    *************************************************************************************/
    private void strokeCircle(int pos_x, int pos_y, int r, Color color)
    {
        double i, angle, x1, y1;
        int x,y;

        for(i = 0; i < 360; i += 0.1)
        {
            angle = i;
            x1 = r * Mathf.Cos((float)(angle * Mathf.PI / 180));
            y1 = r * Mathf.Sin((float)(angle * Mathf.PI / 180));
            x = (int)(pos_x + x1);
            y = (int)(pos_y + y1);

            if(x > 0 && x < _width && y > 0 && y < _height){
                _texture.SetPixel(x, y, color);
            }
        }


    }

    /*************************************************************************************
    * Affichage d'un cercle plein
    * Ajoute le resultat du cercle sur le canvas
    *************************************************************************************/
    private void fillCircle(int pos_x, int pos_y, int r, Color color){

        
        for (int x = -r; x < r ; x++)
        {
            int height = (int)Mathf.Sqrt(r * r - x * x);

            for (int y = -height; y < height; y++){
                _texture.SetPixel(x + pos_x, y + pos_y, color);
            }
        }
    }

    /*************************************************************************************
    * Affichage d'un cercle plein
    * Ajoute le résultat du cercle dans un tableau de couleur temporaire
    *************************************************************************************/
    public static void fillCircle(ref Color[] out_color, int width, int pos_x, int pos_y, int r, Color color){

        
        int coord_x;
        int coord_y;
        int coord;
        int array_length = out_color.Length;
        for (int x = -r; x < r ; x++)
        {
            int height = (int)Mathf.Sqrt(r * r - x * x);

            for (int y = -height; y < height; y++){
                
                coord_x = x + pos_x;
                coord_y = y + pos_y;
                coord = width*coord_y + coord_x;

                if( coord >= 0 && coord < array_length-1){
                    //conversion au passage des coordonées 2D en cordonées sur un tableau plat
                    out_color[coord] = color;
                }
            }
        }
    }

}
