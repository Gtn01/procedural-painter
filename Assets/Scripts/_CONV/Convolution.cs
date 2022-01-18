using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Convolution 
{
   private Color[] _inputColor;
   private int _width;
   private int _height;
   private Color[] _outputColor;
   private float[][] _kernel;
   

   public Convolution(Color[] inputColor, int width, int height, float[][] kernel){
       
       _inputColor = inputColor;
       _kernel = kernel;
       _width  = width;
       _height = height;
   }

   public Color[] apply(float threshold = 0){

       _outputColor = new Color[_inputColor.Length];

       int in_X;
       int in_Y;
       int o_in_X;
       int o_in_Y;
       int o_flat;
       int offset_X;
       int offset_Y;
       int kw = _kernel.Length;
       int kh = _kernel[0].Length;
       
       for( int i = 0 ; i < _inputColor.Length; i++ ){
        
           in_Y = (int)(i/_width);
           in_X = i - (_width * in_Y);
           Color nc = new Color(0,0,0,1); 
           Color cu; 
         
           //string debug = "("+i+") ";

           for( int j = 0 ; j < kw ; j++ ){
               for( int k = 0 ; k < kh ; k++ ){
                   
                   /*
                        0   0   0
                        0   0   0
                        0   0   0
                   */

                   offset_X = j - (int)(kw/2);
                   offset_Y = k - (int)(kh/2); 
                   
                   o_in_X = in_X + offset_X;
                   o_in_Y = in_Y + offset_Y;

                   if(o_in_X >=0 && o_in_X < _width && o_in_Y >=0 && o_in_Y < _height ){

                        o_flat = o_in_Y * _width + o_in_X;
                        cu = _inputColor[o_flat];
                        nc.r += _kernel[j][k] * cu.r;
                        nc.g += _kernel[j][k] * cu.g;
                        nc.b += _kernel[j][k] * cu.b;

                        //debug+="["+j+"/"+offset_X+"/"+o_in_X+","+k+"/"+offset_Y+"/"+o_in_Y+" : "+_kernel[j][k]+"] ";
                        //debug +="+("+cu.r+"*"+_kernel[j][k]+") : "+nc.r;
                   }
                        //debug+="["+j+"]["+k+"]"+_kernel[j][k]+" , ";
                        //debug+="["+o_in_X+","+o_in_Y+"] ";


               }
               //debug += "\n";
               
           } 
               //Debug.Log(debug+" = "+nc.r);

            if(nc.r < threshold){
                nc = Color.black;
            }
           _outputColor[i] = nc;
       }

       return _outputColor;

   }

   public void grayScale(){

       for(int i = 0;i<_inputColor.Length;i++){
           float gray =  (_inputColor[i].r+_inputColor[i].g+_inputColor[i].b)/3.0f;
           _inputColor[i] = new Color(gray,gray,gray,1.0f);
       }
   }

   public static Color[] intersection( Color[] c1, Color[] c2, float threshold){

       Color[] result = new Color[c1.Length];

       for(int i = 0 ; i < c1.Length ; i++ ){

           result[i] = new Color( Mathf.Min(c1[i].r, c2[i].r) , Mathf.Min(c1[i].g, c2[i].g) , Mathf.Min(c1[i].b, c2[i].b) );
   
       }

       return result;
   }
}
