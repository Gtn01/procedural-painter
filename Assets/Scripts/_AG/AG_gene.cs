using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*******************************
*  UN SHOT
*******************************/
public class AG_gene : BrushShot
{   
    // f1 , f2 , position    , length , direction   , color
    // 4o , 4o , ( 4o , 4o ) , 4o     , ( 4o , 4o ) , ( 4o , 4o , 4o ) = 40o
    // 
    public static int AG_GENE_SIZE = 40*8;

    private GT_BitArray _genes; 

    public AG_gene(){

    }

    public AG_gene(BrushShot shot){

        _brush      = shot._brush;
        _f1         = shot._f1;
        _f2         = shot._f2;
        _position   = shot._position;
        _length     = shot._length; 
        _direction  = shot._direction;
        _color      = shot._color;
    }

    public AG_gene(Brush brush, float f1, float f2, Vector2 position, float length, Vector2 direction, Color color) : base(brush, f1, f2, position, length, direction, color)
    {
        _brush      = brush;
        _f1         = f1;
        _f2         = f2;
        _position   = position;
        _length     = length; 
        _direction  = direction;
        _color      = color;
    }

    public void init(){

        //bytes
        List<Byte> bytes = new List<byte>();

        bytes.AddRange( BitConverter.GetBytes(_f1) );
        bytes.AddRange( BitConverter.GetBytes(_f2) );
        bytes.AddRange( BitConverter.GetBytes(_position.x) );
        bytes.AddRange( BitConverter.GetBytes(_position.y) );
        bytes.AddRange( BitConverter.GetBytes(_length) );
        bytes.AddRange( BitConverter.GetBytes(_direction.x) );
        bytes.AddRange( BitConverter.GetBytes(_direction.y) );
        bytes.AddRange( BitConverter.GetBytes(_color.r) );
        bytes.AddRange( BitConverter.GetBytes(_color.g) );
        bytes.AddRange( BitConverter.GetBytes(_color.b) );

        //bits
        _genes = new GT_BitArray( bytes.ToArray() );

    }

    public GT_BitArray genes(){
        if(_genes == null){
            init();
        }
        return _genes;
    }

    public static AG_gene fromBits(GT_BitArray genes, int from, Canvas canvas){
        
        AG_gene gene = new AG_gene();

        byte f1_b1 = genes.GetByte(from+0*8);   
        byte f1_b2 = genes.GetByte(from+1*8);   
        byte f1_b3 = genes.GetByte(from+2*8);   
        byte f1_b4 = genes.GetByte(from+3*8);   
        gene._f1 =  Remap( BitConverter.ToSingle(new[]{f1_b1,f1_b2,f1_b3,f1_b4}, 0) , 1);
        
        byte f2_b1 = genes.GetByte(from+4*8);   
        byte f2_b2 = genes.GetByte(from+5*8);   
        byte f2_b3 = genes.GetByte(from+6*8);   
        byte f2_b4 = genes.GetByte(from+7*8);   
        gene._f2 = Remap( BitConverter.ToSingle(new[]{f2_b1,f2_b2,f2_b3,f2_b4}, 0) , 1);
    
        byte posx_b1 = genes.GetByte(from+8*8);   
        byte posx_b2 = genes.GetByte(from+9*8);   
        byte posx_b3 = genes.GetByte(from+10*8);   
        byte posx_b4 = genes.GetByte(from+11*8);   
        byte posy_b1 = genes.GetByte(from+12*8);   
        byte posy_b2 = genes.GetByte(from+13*8);   
        byte posy_b3 = genes.GetByte(from+14*8);   
        byte posy_b4 = genes.GetByte(from+15*8);   
        gene._position = new Vector2( 
            Remap( BitConverter.ToSingle(new[]{posx_b1,posx_b2,posx_b3,posx_b4}, 0) , canvas._width ) , 
            Remap( BitConverter.ToSingle(new[]{posy_b1,posy_b2,posy_b3,posy_b4}, 0) , canvas._height  ));

        byte length_b1 = genes.GetByte(from+16*8);   
        byte length_b2 = genes.GetByte(from+17*8);   
        byte length_b3 = genes.GetByte(from+18*8);   
        byte length_b4 = genes.GetByte(from+19*8);   
        gene._length =  Remap( BitConverter.ToSingle(new[]{length_b1,length_b2,length_b3,length_b4}, 0), canvas._width);

        byte dirx_b1 = genes.GetByte(from+20*8);   
        byte dirx_b2 = genes.GetByte(from+21*8);   
        byte dirx_b3 = genes.GetByte(from+22*8);   
        byte dirx_b4 = genes.GetByte(from+23*8);   
        byte diry_b1 = genes.GetByte(from+24*8);   
        byte diry_b2 = genes.GetByte(from+25*8);   
        byte diry_b3 = genes.GetByte(from+26*8);   
        byte diry_b4 = genes.GetByte(from+27*8);   
        gene._direction = new Vector2( 
            Remap( BitConverter.ToSingle(new[]{dirx_b1,dirx_b2,dirx_b3, dirx_b4}, 0) , 1), 
            Remap( BitConverter.ToSingle(new[]{diry_b1,diry_b2,diry_b3, diry_b4}, 0) , 1) );


        byte r_b1 = genes.GetByte(from+28*8);   
        byte r_b2 = genes.GetByte(from+29*8);   
        byte r_b3 = genes.GetByte(from+30*8);   
        byte r_b4 = genes.GetByte(from+31*8);   
        byte g_b1 = genes.GetByte(from+32*8);   
        byte g_b2 = genes.GetByte(from+33*8);   
        byte g_b3 = genes.GetByte(from+34*8);   
        byte g_b4 = genes.GetByte(from+35*8);  
        byte b_b1 = genes.GetByte(from+36*8);   
        byte b_b2 = genes.GetByte(from+37*8);   
        byte b_b3 = genes.GetByte(from+38*8);   
        byte b_b4 = genes.GetByte(from+39*8);   
        gene._color = new Color(
            Remap( BitConverter.ToSingle(new[]{r_b1,r_b2, r_b3, r_b4}, 0) , 1) ,
            Remap( BitConverter.ToSingle(new[]{g_b1,g_b2, g_b3, g_b4}, 0) , 1) ,
            Remap( BitConverter.ToSingle(new[]{b_b1,b_b2, b_b3, b_b4}, 0) , 1) );


        return gene;

    }

    /*
    * Remap [intMin - intMax] vers [0 , n]
    */
    public static float Remap (float value, float n) {
        
        value = Math.Abs(value);

        if(value > n){
            return n;
        }

        return value;
    }
    

}
