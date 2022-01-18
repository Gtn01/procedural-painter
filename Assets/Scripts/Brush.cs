using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brush
{
     
    public float _min_thickness; //min pixel size width
    public float _max_thickness; //max pixel size width
    public float _roughness; //coef d'aténuation de la bross [ 1 brosse normal ] [ 0 la brosse n'apparait plus ]

    public Brush(float min, float max, float rough ){

        _min_thickness = min;
        _max_thickness = max;
        _roughness     = rough;
    }

    public float computeThickness(float force){
        //Debug.Log("     "+"("+force +"* ( "+_max_thickness+" -"+ _min_thickness+" )) +"+_min_thickness);
        float thickness = (force * ( _max_thickness - _min_thickness ));
        thickness *= _roughness;
        thickness+= _min_thickness;
        return thickness;
    }
}
