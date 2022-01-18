using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class color_extracterV2 
{

    private Color[] _input_colors;
    public Color[] _output_colors;

    private int _expected_colors;

   public color_extracterV2(Color[] input_colors , int expected_colors){
       _input_colors = input_colors;
       _expected_colors = expected_colors;
       _output_colors = _input_colors;
   }

   public void extract(){

        HashSet<Color> hash_colors = new HashSet<Color>(_input_colors);
        List<Color> list_colors = hash_colors.ToList();
        int currentNbColor = list_colors.Count;
        int[] closest_index = null;

        while(currentNbColor > _expected_colors){
            
            closest_index = closestColorIndex(list_colors);
            Color c1 = list_colors[closest_index[0]];
            Color c2 = list_colors[closest_index[1]];
            Color new_c = merge(c1,c2);
            list_colors.Remove(c1);
            list_colors.Remove(c2);
            list_colors.Add(new_c);
            currentNbColor = list_colors.Count;
        }

        _output_colors = list_colors.ToArray();
   }

    public int[] closestColorIndex(List<Color> c){

        if(c.Count < 2){
            return null;
        } 

        float closest_score  = float.MaxValue;
        int closest_index1 = 0;
        int closest_index2 = 1;

        float score = float.MaxValue;

        for(int i = 0;i<c.Count;i++){
            for(int j = 0;j<c.Count;j++){
                
                if(i == j ){
                    continue;
                }

                score = dist(c[i], c[j]);

                if(score < closest_score){
                    closest_score = score;
                    closest_index1 = i;
                    closest_index2 = j;
                }
            }
        }

        return new int[]{closest_index1, closest_index2};

    }

    public Color merge(Color c1, Color c2){

        return new Color((c1.r+c2.r)/2.0f, (c1.g + c2.g)/2.0f, (c1.b + c2.b)/2.0f);
    }

    public float dist(Color c1, Color c2){

        return Mathf.Abs(c1.r - c2.r) + Mathf.Abs(c1.g - c2.g)  +Mathf.Abs(c1.b - c2.b);

    }
}
