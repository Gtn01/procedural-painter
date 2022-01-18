using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class AG_pool
{
    private Color[] _oeuvre_reference;
    private Brush _brush;
    private Canvas _canvas;
    private int _nbShots;
    private List<AG_adn> _Adns;

    public AG_pool(Color[] oeuvre_reference, Canvas canvas, Brush brush, int nbAdns, int nbShots){
        
        _oeuvre_reference = oeuvre_reference;
        _brush   = brush;
        _canvas  = canvas;
        _nbShots = nbShots;
        _Adns = new List<AG_adn>();

        System.Random random = new System.Random();

        for(int i = 0;i<nbAdns;i++){
            _Adns.Add(AG_adn.Random(canvas, brush, nbShots, random));
        }
    }

    public AG_pool(string fileContent){
        
        string[] lines = fileContent.Split('\n');

        _Adns = new List<AG_adn>();
        
        for(int i = 0;i<lines.Length;i++){
            _Adns.Add(AG_adn.FromFile( lines[i].Trim() ));
        }
   
    }

//    public AG_adn subPool(int start, int size){
//
//        AG_adn p = new AG_adn(size);
//        p._Adns = new List<AG_adn>();
//        p._Adns = _Adns.Skip(start).Take(size).ToList();
//        return p;
//    }

    public void evaluateEach(bool printScores = false)
    {
        foreach(AG_adn adn in _Adns){
            
            adn.Score(_oeuvre_reference,_brush, _canvas, true);
            
            if(printScores){
                Debug.Log(adn.Score(_oeuvre_reference,_brush, _canvas));
            }
        }   
    }

    public void conserveBest(int count){

        evaluateEach();
        Sort();
        _Adns = _Adns.Take(count).ToList();
    }

    public void Sort()
    {
        _Adns.Sort();
    }

    public void Cross(float crossRate)
    {
        System.Random rand = new System.Random();
       
        for(int i = 0;i<_Adns.Count;i++){
            AG_adn adn = _Adns[i];

            if( rand.NextDouble()  < crossRate){
                
                int with = rand.Next(0, _Adns.Count);
                int at   = rand.Next(1, _Adns[with].Count()-1); 
                adn.Cross(_Adns[with], at);
            }
        }  
    }

    public void Mutate(float mutateRate)
    {
        System.Random rand = new System.Random();
        for(int i = 0;i<_Adns.Count;i++){
            AG_adn adn = _Adns[i];

            if( rand.NextDouble() < mutateRate){
                
                int pos = rand.Next(0, adn.Count());
                adn.Flip(pos);
            }
        }  
    }

    public void Renew(float renewRate, float part)
    {  
        System.Random random = new System.Random(); 
        int depart = (int)(_Adns.Count - (_Adns.Count*part));
        depart = depart == 0 ? 1 : depart;

        for(int i = depart ; i<_Adns.Count ; i++){

            if( random.NextDouble() < renewRate){
                _Adns[i] = AG_adn.Random(_canvas, _brush, _nbShots, random);
            }
        }
    }

    public void Shuffle(){
        System.Random rand = new System.Random();
        _Adns.OrderBy(a => rand.Next()).ToList();
    }

    public AG_adn Best(out int index)
    {
        float score = float.MaxValue;
        AG_adn best_adn = null;
        index = 0;

        for(int i = 0;i<_Adns.Count;i++){
            AG_adn adn = _Adns[i];
            
            if( adn.Score(_oeuvre_reference,_brush, _canvas) < score ){
                score    = adn.Score(_oeuvre_reference, _brush,_canvas );
                best_adn = adn;
                index    = i;
            }
        }

        return best_adn;
    }

    public AG_adn Best(){
        return Best(out int index);
    }

    public AG_adn[] NBest(int count){

        _Adns.Sort();
        return _Adns.Take(count).ToArray();
    }

    public int Count(){
        return _Adns.Count;
    }

    public AG_adn Get(int index){
        return _Adns[index];
    }

    public void Set(int index, AG_adn adn){
        _Adns[index] = adn;
    }

    public void SetRange(int index, AG_adn[] adns){
        
        int i = index;
        foreach(AG_adn adn in adns){
            _Adns[i] = adn;
            i++;
        }

    }

    public string ToString(){

        StringBuilder sb = new StringBuilder(); 
        foreach(AG_adn adn in _Adns){
            sb.Append(adn.ToString()+"\n");
        }
        return sb.ToString();
    }


    public string ToFile(){
        
        StringBuilder sb = new StringBuilder();

        foreach(AG_adn adn in _Adns){
            sb.Append(adn.ToFile()+"\n");
        }   

        return sb.ToString().Trim();
    }


    public string ToStringScores(){
        StringBuilder sb = new StringBuilder(); 
        foreach(AG_adn adn in _Adns){
            sb.Append(adn.Score(_oeuvre_reference, _brush, _canvas)+"\n");
        }
        return sb.ToString();

    }
}
