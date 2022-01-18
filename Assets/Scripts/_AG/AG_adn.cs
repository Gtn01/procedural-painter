using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class AG_adn_default_values
{

    public static Color[] _whiteCanvas;

    public static void init(int w, int h)
    {

        _whiteCanvas = new Color[w * h];
        for (int i = 0; i < _whiteCanvas.Length; i++)
        {
            _whiteCanvas[i] = Color.white;
        }

    }

}

/*******************************
*  UNE SERIE DE BRUSH SHOTS
*******************************/
public class AG_adn : IComparable<AG_adn>
{

    //Tous les shots concaténés les un apres les autres
    private GT_BitArray _genesConcatenes;
    public float _score;

    public AG_adn(AG_adn adn)
    {
        _score = 0;
        _genesConcatenes = new GT_BitArray(adn._genesConcatenes);
        _score = adn._score;

    }

    public AG_adn(AG_gene[] adn)
    {

        _score = 0;
        _genesConcatenes = new GT_BitArray(adn.Length * AG_gene.AG_GENE_SIZE);
        int i = 0;

        //Remplissage du tableau de bits avec tous les shots concaténés
        foreach (AG_gene gene in adn)
        {

            for (int j = 0; j < AG_gene.AG_GENE_SIZE; j++)
            {

                _genesConcatenes.Set(i, gene.genes().Get(j));
                i++;
            }
        }
    }

    public static AG_adn Random(Canvas canvas, Brush brush, int nbShots, System.Random random)
    {
        List<AG_gene> genes = new List<AG_gene>();

        for (int i = 0; i < nbShots; i++)
        {
            AG_gene gene = new AG_gene(BrushShot.randomShot(brush, new Vector2(canvas._width, canvas._height), 10, 100, random));
            genes.Add(gene);
        }

        return new AG_adn(genes.ToArray());
    }

    public static AG_adn FromFile(string v)
    {
        throw new NotImplementedException();
    }

    public float ScoreGPU(Color[] reference, Brush brush, Canvas canvas, ComputeShader shader, bool force = false)
    {

        if (_score != 0 && !force)
        {
            return _score;
        }

        System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
        timer.Start();

        Color[] colors = new Color[canvas._width * canvas._height];
        Array.Copy(AG_adn_default_values._whiteCanvas, colors, colors.Length);

        timer.Stop();
        Debug.Log("     init : " + timer.ElapsedMilliseconds + "ms");
        timer.Restart();

        Vector2 dir;
        Vector2 pos;
        float length;

        //ComputeShader shader = (ComputeShader)Resources.Load("ComputeShaders/BrushShotRenderer");
        //shader.SetBuffer(0, "texture", colors);
        //shader.Dispatch(0, _texture.width / 8, _texture.height / 8, 1);

        for (int i = 0; i < _genesConcatenes.Count(); i += AG_gene.AG_GENE_SIZE)
        {
            BrushShot shot = AG_gene.fromBits(_genesConcatenes, i, canvas);
            shot._brush = brush;

            dir = shot._direction.normalized;
            pos = shot._position;
            length = shot._length;

            for (int j = 0; j < length; j++)
            {

                pos = shot._position + j * dir;
                //Canvas.fillCircle(ref colors, canvas._width, (int)pos.x, (int)pos.y, (int)shot.sizeAt(j), shot._color);

            }

        }

        timer.Stop();
        Debug.Log("     draw : " + timer.ElapsedMilliseconds + "ms");
        timer.Restart();

        //ComputeShader shader = (ComputeShader)Resources.Load("ComputeShaders/ImageScoreComputing");
        int shaderKernel = shader.FindKernel("CSMain");

        ComputeBuffer buffer_i1 = new ComputeBuffer(colors.Length, sizeof(float)*4);
        ComputeBuffer buffer_i2 = new ComputeBuffer(reference.Length, sizeof(float)*4);
        buffer_i1.SetData(colors);
        buffer_i2.SetData(reference);
        shader.SetBuffer(shaderKernel, "i1", buffer_i1);
        shader.SetBuffer(shaderKernel, "i2", buffer_i2);

        //Set debug buffer
//       ComputeBuffer bufferDebug = new ComputeBuffer(colors.Length, sizeof(float)*4);
//       Color[] debug = new Color[ colors.Length ];
//       for(int i = 0;i<colors.Length;i++){debug[i] = Color.black;}
//       bufferDebug.SetData(debug);
//       shader.SetBuffer(shaderKernel, "debug", bufferDebug);

        //Set result buffer
        ComputeBuffer buffer_result = new ComputeBuffer(1, sizeof(int));
        buffer_result.SetData( new[]{0} );
        shader.SetBuffer(shaderKernel, "final_result", buffer_result);


        timer.Stop();
        Debug.Log("     prepare : " + timer.ElapsedMilliseconds + "ms");
        timer.Restart();


        //run shader
        shader.Dispatch(shaderKernel, 1024,1, 1);


        //get back data        
        int[] score_output = new int[1];
        buffer_result.GetData(score_output);
        buffer_result.Dispose();

        //get back debug data
//        float debugSum = 0;
//        bufferDebug.GetData(debug);
//        bufferDebug.Dispose();
//        StringBuilder sb = new StringBuilder();
//        foreach(Color c in debug){
//            sb.Append(c.r+" , ");
//            debugSum += c.r;
//        }
//        Debug.Log(debugSum+" : "+sb.ToString());

        _score = score_output[0]/1000;

        timer.Stop();
        Debug.Log("     diff : " + timer.ElapsedMilliseconds + "ms");
        timer.Restart();
        return _score;
    }

    public float Score(Color[] reference, Brush brush, Canvas canvas, bool force = false)
    {
        if (_score != 0 && !force)
        {
            return _score;
        }

        //System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
        //timer.Start();

        Color[] colors = new Color[canvas._width * canvas._height];
        Array.Copy(AG_adn_default_values._whiteCanvas, colors, colors.Length);

        //timer.Stop();
        //Debug.Log("     init : " + timer.ElapsedMilliseconds + "ms");
        //timer.Restart();

        int nbGenes = _genesConcatenes.Count();
        Vector2 dir;
        Vector2 pos;
        float length;
        BrushShot shot;
        
        for (int i = 0; i < nbGenes; i += AG_gene.AG_GENE_SIZE)
        {
            shot = AG_gene.fromBits(_genesConcatenes, i, canvas);
            shot._brush = brush;

            dir = shot._direction.normalized;
            pos = shot._position;
            length = shot._length;

            for (int j = 0; j < length; j+=20)
            {
                pos = shot._position + j * dir;
                Canvas.fillCircle(ref colors, canvas._width, (int)pos.x, (int)pos.y, (int)shot.sizeAt(j), shot._color);

            }

        }

        //timer.Stop();
        //Debug.Log("     draw : " + timer.ElapsedMilliseconds + "ms");
        //timer.Restart();

        float score = 0;

        for (int i = 0; i < colors.Length; i+=10)
        {
            score += Math.Abs( reference[i].r - colors[i].r );
            score += Math.Abs( reference[i].g - colors[i].g );
            score += Math.Abs( reference[i].b - colors[i].b );
        }

        _score = score;

        //timer.Stop();
        //Debug.Log("     diff : " + timer.ElapsedMilliseconds + "ms");
        //timer.Restart();
        return _score;

    }

    public Texture2D Render(Canvas canvas, Brush brush)
    {

        Color[] colors = new Color[canvas._width * canvas._height];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = Color.white;
        }

        Vector2 dir;
        Vector2 pos;
        float length;

        for (int i = 0; i < _genesConcatenes.Count(); i += AG_gene.AG_GENE_SIZE)
        {

            BrushShot shot = AG_gene.fromBits(_genesConcatenes, i, canvas);
            shot._brush = brush;

            dir = shot._direction.normalized;
            pos = shot._position;
            length = shot._length;

            for (int j = 0; j < length; j+=20)
            {

                pos = shot._position + j * dir;
                Canvas.fillCircle(ref colors, canvas._width, (int)pos.x, (int)pos.y, (int)shot.sizeAt(j), shot._color);

            }

        }

        Texture2D tex = new Texture2D(canvas._width, canvas._height);
        tex.SetPixels(colors);
        tex.Apply();

        return tex;
    }

    public void Cross(AG_adn with, int at)
    {
        GT_BitArray g1 = _genesConcatenes;
        GT_BitArray g2 = with._genesConcatenes;
        GT_BitArray g_child = new GT_BitArray(_genesConcatenes.Count());

        for (int i = 0; i < at; i++)
        {
            g_child.Set(i, g1.Get(i));
        }

        for (int i = at; i < g2.Count(); i++)
        {
            g_child.Set(i, g2.Get(i));
        }

        _genesConcatenes = g_child;
    }

    public int Count()
    {
        return _genesConcatenes.Count();
    }

    public void Flip(int pos)
    {
        _genesConcatenes.Set(pos, !_genesConcatenes.Get(pos));
    }

    public string ToFile()
    {
        StringBuilder sb = new StringBuilder();

        for(int i = 0;i<_genesConcatenes.Count();i++)
        {
            char b = _genesConcatenes.Get(i) ? '1' : '0';
            sb.Append(b);
        }

        return sb.ToString().Trim();
    }

    public static AG_adn[] ArrayCopy(AG_adn[] adns)
    {
        List<AG_adn> ret = new List<AG_adn>();
        foreach (AG_adn adn in adns)
        {
            ret.Add(new AG_adn(adn));
        }
        return ret.ToArray();
    }

    public int CompareTo(AG_adn other)
    {
        if (_score < other._score)
        {
            return -1;
        }
        else if (_score == other._score)
        {
            return 0;
        }
        else
        {
            return 1;
        }
    }
}
