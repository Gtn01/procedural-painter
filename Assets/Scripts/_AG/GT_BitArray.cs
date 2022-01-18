using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GT_BitArray 
{
    private bool[] _data;

    public GT_BitArray(){
        _data = new bool[0];
    }

    public GT_BitArray(int length){
        _data = new bool[length];
    }

    public GT_BitArray(GT_BitArray data_in){
        _data = new bool[data_in._data.Length];
        Array.Copy(data_in._data, _data, _data.Length);
    }

    public GT_BitArray(byte[] data_in){
        
        int length = data_in.Length;
        List<bool> tmp_data = new List<bool>();

        for(int i = 0; i < length;i++){
            tmp_data.AddRange( byteToBool(data_in[i]) );
        }

        _data = tmp_data.ToArray();

    }

    public bool Get(int index){
        return _data[index];
    }

    public void Set(int index, bool value){
        _data[index] = value;
    }

    public int Count(){
        return _data.Length;
    }

    public byte GetByte(int index){
        return boolToByte(new[]{
            _data[index+0],  
            _data[index+1], 
            _data[index+2],
            _data[index+3], 
            _data[index+4], 
            _data[index+5], 
            _data[index+6], 
            _data[index+7]
        });
    }


    private static byte boolToByte(bool[] source)
    {
        byte result = 0;
        // This assumes the array never contains more than 8 elements!
        int index = 8 - source.Length;

        // Loop through the array
        foreach (bool b in source)
        {
            // if the element is 'true' set the bit at that position
            if (b){
                result |= (byte)(1 << (7 - index));
            }

            index++;
        }

        return result;
    }

    private static bool[] byteToBool(byte b)
    {
        // prepare the return result
        bool[] result = new bool[8];

        // check each bit in the byte. if 1 set to true, if 0 set to false
        for (int i = 0; i < 8; i++)
            result[i] = (b & (1 << i)) == 0 ? false : true;

        // reverse the array
        Array.Reverse(result);

        return result;
    }
}
