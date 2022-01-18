using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class AG_trainer{
    
    private float _Elistist = 0.15f;
    private float _MutateRate = 0.8f;
    private float _CrossRate = 0.8f;
    private float _RenewRate = 0.5f;
    private float _RenewPart = 0.2f;
    private AG_pool _Pool;


    public AG_trainer( Color[] oeuvreReference, Canvas canvas, Brush brush, int poolSize, int nbShots){
        

        _Pool = new AG_pool(oeuvreReference, canvas, brush , poolSize, nbShots);

    }

    public AG_trainer(AG_pool pool){
        _Pool = pool;
    }

    public void RunTraining(int nbIteration, Action<AG_adn, int, AG_pool> progress, Action<AG_pool, AG_adn> done){

        Thread training_thread = new Thread(()=>{
            
            //System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            //timer.Start();

            for(int i = 0;i<nbIteration;i++){

                AG_adn best = TrainIteration();
                progress(best, i, _Pool);

            }

            done(_Pool, _Pool.Best(out int index));

            //timer.Stop();
            //Debug.Log("Complete : "+timer.ElapsedMilliseconds+" ms");
        
        });

        training_thread.Priority = System.Threading.ThreadPriority.Highest;
        training_thread.Start();

    }

    public AG_adn TrainIteration(){

        System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
        timer.Start();

        _Pool.evaluateEach();

        //timer.Stop();
        //Debug.Log(" Evaluation : "+timer.ElapsedMilliseconds+" ms");
        //timer.Restart();

        int nbBest = (int)(_Elistist * _Pool.Count());
        nbBest = nbBest == 0 && _Elistist > 0 ? 1 : nbBest;
        AG_adn[] bests = AG_adn.ArrayCopy( _Pool.NBest(nbBest) );
        
        //timer.Stop();
        //Debug.Log(" Best : "+timer.ElapsedMilliseconds+" ms");
        //timer.Restart();

        _Pool.Shuffle();

        //timer.Stop();
        //Debug.Log(" Shuffle : "+timer.ElapsedMilliseconds+" ms");
        //timer.Restart();

        _Pool.Cross(_CrossRate);

        //timer.Stop();
        //Debug.Log(" Cross : "+timer.ElapsedMilliseconds+" ms");
        //timer.Restart();

        _Pool.Mutate(_MutateRate);

        //timer.Stop();
        //Debug.Log(" Mutate : "+timer.ElapsedMilliseconds+" ms");
        //timer.Restart();

        _Pool.Renew(_RenewRate , _RenewPart);  

        //timer.Stop();
        //Debug.Log(" Renew : "+timer.ElapsedMilliseconds+" ms");
        //timer.Restart();  

        if(_Elistist > 0){
            _Pool.SetRange(0, bests);
        }

        //timer.Stop();
        //Debug.Log(" Elistist : "+timer.ElapsedMilliseconds+" ms");

        timer.Stop();
        Debug.Log(" Train : "+timer.ElapsedMilliseconds+" ms");

        return _Pool.Best(out int index);

    }
}
