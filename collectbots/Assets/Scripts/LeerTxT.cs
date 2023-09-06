using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.Networking;
using static LeerTxT;


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
    private int[,] data, basurasPos;


    [SerializeField] GameObject terrenoPrefab;
    [SerializeField] GameObject papeleraPrefab;
    [SerializeField] GameObject robotPrefab;
    [SerializeField] GameObject[] mueblesPrefab, basuraPrefab, basuras;
    //[SerializeField] GameObject camara;

    private void Start()
    {
        //Genera todo con el texto
        obtenerMedidasyMatriz();
        generarTerreno(terrenoPrefab);
        generarPapelera(papeleraPrefab);
        generarObstaculos(mueblesPrefab);
        generarRobots(robotPrefab);
        generarBasura(basuraPrefab);

        //Genera todo con el sevidor
        StartCoroutine(Simular());
        
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

                string[] lines = jsonResponse.Split('\n');

                if (lines.Length >= 1)
                {
                    string[] firstLineValues = lines[0].Split(' '); // Suponiendo que los valores están separados por espacios
                    if (firstLineValues.Length >= 2)
                    {
                        height = int.Parse(firstLineValues[0]);
                        width = int.Parse(firstLineValues[1]);

                    }
                }

                // Crear una matriz para el resto de los datos
                data = new int[height, width];

                // Almacenar el resto de los datos en la matriz
                for (int i = 1; i < Mathf.Min(lines.Length, height + 1); i++)
                {
                    string[] rowValues = lines[i].Split(' '); 

                    for (int j = 0; j < Mathf.Min(rowValues.Length, width); j++)
                    {
                        if (j < rowValues.Length)
                        {
                            data[i - 1, j] = int.Parse(rowValues[j]);

                            if (data[i - 1, j] >= 100 && data[i - 1, j] <= 104)
                            {
                                GameObject[] robots = GameObject.FindGameObjectsWithTag("Robot");
                                
                                foreach (var robot in robots)
                                {
                                    RobotScript robotScript = robot.GetComponent<RobotScript>();
                                    if (robotScript != null && robotScript.ID == data[i - 1, j])
                                    {
                                        robot.transform.position = new Vector3(j, 0.344f, -1 * (i - 1));                 
                                    }
                                }
                            }            
                            
                            if (data[i - 1, j] == 400)
                            {
                                foreach (var basura in basuras)
                                {
                                    if(basura.transform.position.x == j && basura.transform.position.z == -1 * (i - 1))
                                    {
                                        basura.SetActive(false);
                                    }
                                }
                            }
                        }
                    }

                }

            }
        }

        yield return new WaitForSeconds(0.1f);
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

                Debug.Log("Iteraciones: " + jsonResponse);

                iteraciones = int.Parse(jsonResponse);
            }
        }
        yield return null;
    }

    public IEnumerator Simular()
    { 
        yield return StartCoroutine(obtIteraciones());
        Debug.Log("Iteraciones" + iteraciones.ToString());
        for (int i = 0; i < iteraciones; i++)
        {
            yield return StartCoroutine(obtenerApi(i));            
        }        
    }

    public enum idPrefab
    {
        Papelera = 300,
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
        int contadorBasura = 0;
        for (int i = 0;i < height; i++)
        {
            for(int j= 0; j < width;j++)
            {
                if (matrix[i, j] == "X")
                {
                    contadorBasura++;
                }
            }
        }
        basurasPos = new int[contadorBasura, 2];
        int auxCont = 0;
        for (int i = 0; i < height; i++)
        {            
            for (int j = 0; j < width; j++)
            {
                if (matrix[i, j] == "X" && auxCont <= contadorBasura)
                {
                    basurasPos[auxCont, 0] = i;
                    basurasPos[auxCont, 1] = j;
                    auxCont++;
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
                    
                    // Asignar el ID de la papelera utilizando el enum
                    idPrefab papeleraID = idPrefab.Papelera;
                    int id = (int)papeleraID;        
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
                    int index = Random.Range(0, obstaculos.Length);
                    GameObject aux = obstaculos[index];
                    Instantiate(aux, new Vector3(i, 0, -1 * j), new Quaternion(0,0,0,1));
                }
            }
        }
    }

    public void generarRobots(GameObject robotBPrefab)
    {
        // Iniciar con el ID Robot1

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (matrix[j, i] == "S") // Detectar la letra 'S' en la matriz
                {
                    // Generar 5 robots alrededor de la posición (i, j)
                    for (int k = 100; k < 105; k++)
                    {
                        GameObject robot = Instantiate(robotBPrefab, new Vector3(i, 0.344f, -1 * j), Quaternion.identity);

                        RobotScript robotScript = robot.AddComponent<RobotScript>();

                        robotScript.ID = k;                        
                    }
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
                    int index = Random.Range(0, basura.Length);
                    GameObject aux = basura[index];

                    // Generar el prefab en la posición (i, j)
                    Instantiate(aux, new Vector3(i, 0, -1 * j), Quaternion.identity);
                }
            }
        }

        basuras = GameObject.FindGameObjectsWithTag("Basura");
    }
}
