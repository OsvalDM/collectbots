using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.Networking;


public class LeerTxT : MonoBehaviour
{
    [TextArea(100, 100)]
    public string content_txt;
    public Text outputText;
    // Start is called before the first frame update

    private int height = 0;
    private int width = 0;
    private int iteraciones = 0;
    private string[,] matrix;
    private int[,] data;

    [SerializeField] GameObject terrenoPrefab;
    [SerializeField] GameObject papeleraPrefab;
    [SerializeField] GameObject robotPrefab;
    [SerializeField] GameObject[] mueblesPrefab, basuraPrefab;
    //[SerializeField] GameObject camara;

    private void Start()
    {
        obtenerMedidasyMatriz();
        generarTerreno(terrenoPrefab);
        generarPapelera(papeleraPrefab);
        generarObstaculos(mueblesPrefab);
        generarRobots(robotPrefab);
        generarBasura(basuraPrefab);

        //ajustarCamara();
        simular();
    }

    public void obtenerMedidasyMatriz()
    {
        string[] lines = content_txt.Split('\n');

        string[] dimentionElement = lines[0].Split(' ');
        height = int.Parse(dimentionElement[0]);
        width = int.Parse(dimentionElement[1]);

        matrix = new string[height, width];


        for (int i = 0; i < height; i++)
        {
            string[] aux = lines[i + 1].Split(' ');
            for (int j = 0; j < width; j++)
            {
                if (j < aux.Length)
                {
                    matrix[i, j] = aux[j];
                }
            }
        }
        outputText.text = $"Altura: {height}\nAncho: {width}";        
    }

    public void generarTerreno(GameObject terreno)
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Instantiate(terreno, new Vector3(i, 0, -1 * j), new Quaternion(0, 0, 0, 1));
            }
        }
    }


    public void generarPapelera(GameObject papelera)
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (matrix[j, i] == "P") // Aquí detectamos la letra 'P' en la matriz
                {
                    Instantiate(papelera, new Vector3(i, 0, -1 * j), new Quaternion(0, 0, 0, 1));
                }
            }
        }
    }

    public void generarObstaculos(GameObject[] obstaculos)
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (matrix[j, i] == "X") // Aquí detectamos la letra 'X' en la matriz
                {
                    int index = Random.Range(0, obstaculos.Length - 1);
                    GameObject aux = obstaculos[index];
                    Instantiate(aux, new Vector3(i, 0, -1 * j), new Quaternion(0,0,0,1));
                }
            }
        }
    }

    public void generarRobots(GameObject robotBPrefab)
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (matrix[j, i] == "S") // Detectar la letra 'S' en la matriz
                {
                    // Generar 4 robots alrededor de la posición (i, j)
                    Instantiate(robotBPrefab, new Vector3(i, 0.344f, -1 * j), Quaternion.identity);
                    Instantiate(robotBPrefab, new Vector3(i, 0.344f, -1 * j), Quaternion.identity);
                    Instantiate(robotBPrefab, new Vector3(i, 0.344f, -1 * j), Quaternion.identity);
                    Instantiate(robotBPrefab, new Vector3(i, 0.344f, -1 * j), Quaternion.identity);
                    Instantiate(robotBPrefab, new Vector3(i, 0.344f, -1 * j), Quaternion.identity);
                }
            }
        }
    }

    public void generarBasura(GameObject[] basura)
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (int.TryParse(matrix[j, i], out int valor) && valor > 0)
                {
                    int index = Random.Range(0, basura.Length - 1);
                    GameObject aux = basura[index];

                    // Generar el prefab en la posición (i, j)
                    Instantiate(aux, new Vector3(i, 0, -1 * j), Quaternion.identity);
                }
            }
        }
    }

    /*public void ajustarCamara()
    {
        float areaAux = 13 * 7;
        float areaActual = width * height;
        float fieldOfV = areaActual * 60 / areaAux;

        camara.GetComponent<Camera>().fieldOfView = fieldOfV;
    }*/

    public void simular()
    {
        data = new int[height, width];
        StartCoroutine(obtIteraciones());
        //for (int i = 0; i < iteraciones; i++)
        //{
        //    StartCoroutine(obtenerApi(i));
        //      --> mover robots --> verificar basura
        //}
        StartCoroutine(obtenerApi(1));
    }

    IEnumerator obtenerApi(int iteracion)
    {

        string url = "http://localhost:8585/grid?ite=" + iteracion.ToString();
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();
            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log("Error");
                Debug.Log(www.error);
            }
            else
            {
                string jsonResponse = www.downloadHandler.text;

                Debug.Log(jsonResponse);

                
            }
        }

    }

    IEnumerator obtIteraciones()
    {

        string url = "http://localhost:8585/iterations";
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();
            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log("Error");
                Debug.Log(www.error);
            }
            else
            {
                string jsonResponse = www.downloadHandler.text;

                Debug.Log("iteraciones: " + jsonResponse);

                iteraciones = int.Parse(jsonResponse);
            }
        }

    }
}
