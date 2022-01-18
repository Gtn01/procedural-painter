using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Masterpiece
{
    public int _width;
    public int _height;
    public Brush _brush;
    public List<BrushShot> _burshShots;


    public Masterpiece(int width, int height, Brush brush)
    {

        _width = width;
        _height = height;
        _brush = brush;
        _burshShots = new List<BrushShot>();

    }

    public static Masterpiece randomPaint(int width, int height, Brush brush, int nbShots)
    {
        System.Random random = new System.Random();
        Masterpiece masterpiece = new Masterpiece(width, height, brush);
        
        for (int i = 0; i < nbShots; i++)
        {
            masterpiece.addShot(BrushShot.randomShot(brush, new Vector2(width, height), 10, 100, random));
        }

        return masterpiece;
    }

    /*************************************************************************************
    * Calcul la texture avec tous les coupe de pinceau
    *************************************************************************************/
    public Canvas renderPiece()
    {

        Canvas canvas = new Canvas(_width, _height);

        canvas.addShots(_burshShots.ToArray());

        return canvas;
    }

    public Canvas renderGPU()
    {

        Canvas canvas = new Canvas(_width, _height);
        ComputeShader shader = (ComputeShader)Resources.Load("ComputeShaders/MasterPieceRenderer");

        //Passe l'info au shader : [id] s'il y a plusieurs instance de ce shader , le nom de la texture dans le shader, reference de la texture unity
        shader.SetTexture(0, "Canvas", canvas._texture);

        //[32x32x1] (1024) Groupe de -> [8x8x1] (64) threads
        // on retoruve bien 1024*65 = 65635 threads
        //la résolution de la terxture était de 256*256 = 65635
        shader.Dispatch(0, _burshShots.Count / 10, 1, 1);

        return canvas;
    }

    /*************************************************************************************
    * Ajout un coup de pinceau sur la masterpiece
    *************************************************************************************/
    public void addShot(float f1, float f2, Vector2 position, float length, Vector2 direction, Color color)
    {


        _burshShots.Add(new BrushShot(_brush, f1, f2, position, length, direction, color));

    }

    /*************************************************************************************
    * Ajoute un coup de pinceau déja prédéfinit, utilisé uniquement quand le coup de pinceau
    * est générer en amont dans le code (type random)
    *************************************************************************************/
    public void addShot(BrushShot brushShot)
    {

        _burshShots.Add(brushShot);
    }

    /*************************************************************************************
    * Supprime tous les coup de pinceau, le prochain rendu sera blanc
    *************************************************************************************/
    public void clear()
    {

        _burshShots.Clear();

    }

}
