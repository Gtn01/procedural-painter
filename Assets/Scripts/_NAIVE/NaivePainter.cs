using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class NaivePainter
{
    private Brush _brush;
    private int _brushMaxLength;
    private Color[] _reference;
    private Color[] _stensil;
    private Color[] _palette;
    private Color[] _texture;
    private Stack<int> _white_pixels;
    private int _width;
    private int _height;
    private System.Random _random;
    private float _colorDiffThreshold = 2.5f;

    private List<BrushShot> appliedShots;


    /*
    * Timers
    */
    System.Diagnostics.Stopwatch timer_init;
    System.Diagnostics.Stopwatch timer_search;
    System.Diagnostics.Stopwatch timer_paintFrom;
    System.Diagnostics.Stopwatch timer_apply;
    System.Diagnostics.Stopwatch timer_global;


    public NaivePainter(Color[] reference, int width, int height, Brush brush)
    {

        _texture = new Color[width * height];

        int[] indices = new int[width * height];

        for (int i = 0; i < _texture.Length; i++)
        {
            _texture[i] = Color.white;
            indices[i] = i;
        }

        _white_pixels = new Stack<int>(indices);
        System.Random Rand = new System.Random();
        int size = _white_pixels.Count;
        _white_pixels = new Stack<int>(_white_pixels.OrderBy((o) => { return (Rand.Next() % size); }));

        _reference = reference;
        _stensil = null;
        _width = width;
        _height = height;
        _brush = brush;
        _random = new System.Random();
        appliedShots = new List<BrushShot>();
    }

    public NaivePainter(Color[] reference, Color[] stensil, int width, int height, Brush brush)
    {

        _texture = new Color[width * height];

        int[] indices = new int[width * height];

        for (int i = 0; i < _texture.Length; i++)
        {
            _texture[i] = Color.white;
            indices[i] = i;
        }

        _white_pixels = new Stack<int>(indices);

        //Shuffle the list
        System.Random Rand = new System.Random();
        int size = _white_pixels.Count;
        _white_pixels = new Stack<int>(_white_pixels.OrderBy((o) => { return (Rand.Next() % size); }));

        _reference = reference;
        _stensil = stensil;
        _width = width;
        _height = height;
        _brush = brush;
        _random = new System.Random();
        appliedShots = new List<BrushShot>();
    }

    public NaivePainter(Color[] reference, int width, int height, Brush brush, Color[] palette)
    {

        _texture = new Color[width * height];

        int[] indices = new int[width * height];

        for (int i = 0; i < _texture.Length; i++)
        {
            _texture[i] = Color.white;
            indices[i] = i;
        }

        _white_pixels = new Stack<int>(indices);
        System.Random Rand = new System.Random();
        int size = _white_pixels.Count;
        _white_pixels = new Stack<int>(_white_pixels.OrderBy((o) => { return (Rand.Next() % size); }));

        _reference = reference;
        _palette = palette;
        _width = width;
        _height = height;
        _brush = brush;
        _random = new System.Random();
        appliedShots = new List<BrushShot>();
    }

    public NaivePainter(Color[] reference, Color[] stensil, int width, int height, Brush brush, int brushMaxLength, Color[] palette)
    {

        timer_global = new System.Diagnostics.Stopwatch();
        timer_init = new System.Diagnostics.Stopwatch();
        timer_search = new System.Diagnostics.Stopwatch();
        timer_paintFrom = new System.Diagnostics.Stopwatch();
        timer_apply = new System.Diagnostics.Stopwatch();

        timer_init.Start();


        _texture = new Color[width * height];

        int[] indices = new int[width * height];

        for (int i = 0; i < _texture.Length; i++)
        {
            _texture[i] = Color.white;
            indices[i] = i;
        }

        _white_pixels = new Stack<int>(indices);
        System.Random Rand = new System.Random();
        int size = _white_pixels.Count;
        _white_pixels = new Stack<int>(_white_pixels.OrderBy((o) => { return (Rand.Next() % size); }));

        _reference = reference;
        _stensil = stensil;
        _palette = palette;
        _width = width;
        _height = height;
        _brush = brush;
        _brushMaxLength = brushMaxLength;
        _random = new System.Random();
        appliedShots = new List<BrushShot>();

        timer_init.Stop();
    }

    /*************************************************************************************
     * Lance le thread pour le calcul du rendu du tableau
     *************************************************************************************/
    public void runNaiveThread(Action<Color[]> progress, Action<NaivePainter> done)
    {

        //TODO a reprendre aussi
        //       new Thread(() => 
        //       {
        //           Thread.CurrentThread.IsBackground = true; 
        //
        //           int maxPaint = 10000;
        //           int next = searchNext();
        //           
        //           while(next > -1 && maxPaint > 0){
        //
        //               BrushShot nextShot = paintFrom(next, 80);                
        //               appliedShots.Add(applyShot(nextShot));
        //               
        //               next = searchNext();
        //
        //               progress(_texture);
        //               maxPaint --;
        //               //Debug.Log(appliedShots.Count);
        //
        //           }
        //
        //           done(this);
        //
        //       }).Start();
    }

    public Color[] runNaive(int brushStrokeNumber)
    {

        timer_global.Start();

        int maxPaint = brushStrokeNumber;
        int next = searchNext();

        while (next > -1 && maxPaint > 0)
        {

            BrushShot nextShot = paintFrom(next, _brushMaxLength);
            appliedShots.Add(applyShot(nextShot));

            next = searchNext();

            maxPaint--;

        }

        timer_global.Stop();
        Debug.Log("Global : " + timer_global.ElapsedMilliseconds);
        Debug.Log("     init : " + timer_init.ElapsedMilliseconds);
        Debug.Log("     search : " + timer_search.ElapsedMilliseconds);
        Debug.Log("     painFrom : " + timer_paintFrom.ElapsedMilliseconds);
        Debug.Log("     apply : " + timer_apply.ElapsedMilliseconds);

        return _texture;

    }



    /*************************************************************************************
     * Recherche le prochain point de départ pour le prochain coup de pineau
     *************************************************************************************/
    public int searchNext()
    {

        timer_search.Start();

        int nbWhitePixel = _white_pixels.Count;

        if(_white_pixels == null || nbWhitePixel == 0){
            return -1;
        }

        int index = _white_pixels.Pop();
        nbWhitePixel --;
        
        if(_stensil == null){
            while( (_texture[index] != Color.white || (_reference[index].r > 0.9 && _reference[index].g > 0.9 && _reference[index].b > 0.9 ) ) && nbWhitePixel > 0){
                index = _white_pixels.Pop();
                nbWhitePixel --;
            }
        }else{

            while( (_stensil[index] == Color.black || ( _reference[index].r > 0.9 && _reference[index].g > 0.9 && _reference[index].b > 0.9 ) ) &&  nbWhitePixel > 0){
                index = _white_pixels.Pop();
                nbWhitePixel--;

            }
        }

        timer_search.Stop();
        return index;
    }

    /*************************************************************************************
     * Création d'un brushshot a partir d'un point sur le tableau
     *************************************************************************************/
    public BrushShot paintFrom(int i, int brushMaxLength)
    {

        timer_paintFrom.Start();

        BrushShot shot = BrushShot.randomShot(_brush, new Vector2(_width, _height), 5, brushMaxLength, _random);
        int y = i / _width;
        int x = i - (y * _width);

        shot._position = new Vector2(x, y);

        if (_palette == null)
        {
            shot._color = _reference[i];
        }
        else
        {

            shot._color = closestColor(_reference[i]);
        }

        timer_paintFrom.Stop();

        return shot;
    }

    /*************************************************************************************
     * Applique un brush shot sur la texture courante
     * il peut stoper le shot s'il croise un pb 
     *************************************************************************************/
    public BrushShot applyShot(BrushShot shot)
    {

        timer_apply.Start();

        Vector2 pos = shot._position;
        Vector2 dir = shot._direction.normalized;

        for (int j = 0; j < shot._length; j++)
        {

            pos = shot._position + j * dir;

            int flatPosition = (int)(pos.y * _width + pos.x);


            try
            {
                float colorDiff = Mathf.Abs(_reference[flatPosition].r - shot._color.r) +
                                Mathf.Abs(_reference[flatPosition].g - shot._color.g) +
                                Mathf.Abs(_reference[flatPosition].b - shot._color.b);


                if (colorDiff < _colorDiffThreshold)
                {
                    fillCircle(_width, (int)pos.x, (int)pos.y, (int)shot.sizeAt(j), shot._color);

                }
                else
                {

                    shot._length = j;
                    return shot;
                }

            }
            catch (Exception e)
            {
                shot._length = j;
                return shot;
            }

        }


        timer_apply.Stop();

        return shot;

    }

    /*************************************************************************************
    * Affichage d'un cercle plein
    * Ajoute le résultat du cercle dans un tableau de couleur temporaire
    *************************************************************************************/
    public void fillCircle(int width, int pos_x, int pos_y, int r, Color color)
    {


        int coord_x;
        int coord_y;
        int coord;
        int array_length = _texture.Length;
        for (int x = -r; x < r; x++)
        {
            int height = (int)Mathf.Sqrt(r * r - x * x);

            for (int y = -height; y < height; y++)
            {

                coord_x = x + pos_x;
                coord_y = y + pos_y;
                coord = width * coord_y + coord_x;

                if (coord >= 0 && coord < array_length - 1)
                {
                    //conversion au passage des coordonées 2D en cordonées sur un tableau plat
                    _texture[coord] = color;
                }
            }
        }
    }

    public Color closestColor(Color c_ref)
    {

        if (_palette == null)
        {
            return Color.black;
        }

        float best_scrore = float.MaxValue;
        Color best_color = Color.black;
        float diff;

        foreach (Color c in _palette)
        {
            diff = Mathf.Abs(c.r - c_ref.r) + Mathf.Abs(c.g - c_ref.g) + Mathf.Abs(c.b - c_ref.b);

            if (diff < best_scrore)
            {
                best_color = c;
                best_scrore = diff;
            }
        }

        return best_color;


    }

}
