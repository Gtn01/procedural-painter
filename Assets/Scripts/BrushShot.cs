using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrushShot : IComparable<BrushShot>
{
   public Brush _brush;

   public float _f1; //Force au debut du trace [0,1]
   public float _f2; //Force au milieu du trace [0,1]
   public Vector2 _position; //Position de départ en px sur le canvas
   public float _length; //Longueur du trait
   public Vector2 _direction; //Direction de la brosse en 2D
   public Color _color; //Couleur du coup de pinceau


   public BrushShot(){
      _brush = null;
   }
   public BrushShot(Brush brush, float f1, float f2, Vector2 position, float length, Vector2 direction, Color color){

      _brush      = brush;
      _f1         = f1;
      _f2         = f2;
      _position   = position;
      _length     = length; 
      _direction  = direction;
      _color = color;
      
   }

    /*************************************************************************************
    * Genere un coup de pinceau aléatoire
    *************************************************************************************/
    internal static BrushShot randomShot(Brush brush, Vector2 posMax, float minLength, float maxLength, System.Random rand = null)
    {    
         if(rand == null){
            rand = new System.Random();
         }

         Vector2 position  = new Vector2((float)(rand.NextDouble()*posMax.x), (float)(rand.NextDouble()*posMax.y));
         Vector2 direction = new Vector2((float)(rand.NextDouble()*posMax.x), (float)(rand.NextDouble()*posMax.y)).normalized;
         Color color       = new Color((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble());
         float length      = (float)rand.NextDouble()*(maxLength-minLength)+minLength;
         float f1          = (float)rand.NextDouble();
         float f2          = (float)rand.NextDouble();

         //Debug.Log("[ "+f1+" , "+f2+" , "+f3+" , "+length+" , "+color+" , "+direction+" , "+position);


         return new BrushShot(brush, f1 , f2  ,position, length,  direction, color );

    }

    /*************************************************************************************
    * Calcul la largeur du coup de pinceau a in instant T suyr la longueur du trait
    *************************************************************************************/
    public float sizeAt(float at){
      
      float force = 0;

      force = Mathf.Lerp(_f1, _f2, at/_length);

      return _brush.computeThickness(force);

   }

   public string ToString(){
      return _f1+" \n"+_f2+" \n"+_position+" \n"+_length+" \n"+_direction+" \n"+_color;
   }

    /*************************************************************************************
    * Implémanentation du IComparable 
    *************************************************************************************/
    public int CompareTo(BrushShot other)
    {

       if( (int)_position.x == (int)other._position.x && 
           (int)_position.y == (int)other._position.y && 
           (int)_length == (int)other._length && 
           (int)_direction.x == (int)other._direction.x &&
           (int)_direction.y == (int)other._direction.y){
          return 0;
       }else{
          return 1;
       }
    }

}
