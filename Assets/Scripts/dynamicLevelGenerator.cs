using System.Collections;
using UnityEngine;

public class dynamicLevelGenerator : MonoBehaviour
{
    public static dynamicLevelGenerator levelGenerator; //singleton

    [Header("Seed")]
    [SerializeField] int generatorSeed; //seed for generator

    [Header("Prefabs")]
    [SerializeField] GameObject columnPrefab; //column prefab
    [SerializeField] GameObject circlePrefab; //circle prefab
    [SerializeField] GameObject portalWallPrefab; //portal wall prefab

    [Header("Values")]
    [SerializeField] int numberOfColumns; //number of columns to spawn
    [SerializeField] float lastHeight; //last height of bottom column
    [SerializeField] float lastDistance; //last distance between bottom and top columns
    [SerializeField] float minDistance; //minimum disatnce between bottom and top columns
    [SerializeField] float maxDistance; //maximum distance between bottom and top columns
    [SerializeField] float randomRange; //randomization range

    private void Awake() //on wake
    {
        levelGenerator = this; //set singleton
    }

    private void Start() //on start
    {
        StartCoroutine(generateLevel()); //start generation coroutine
    }

    IEnumerator generateLevel() //generation coroutine
    {
        Color phaseColor = Color.blue; //set first color to blue

        Random.InitState(generatorSeed); //set seed to generator seed

        for (int i = 0; i < numberOfColumns; i++) //loop for number of columns
        {
            float newHeight = lastHeight + Random.Range(-randomRange, randomRange); //randomize new height
            float newDistance; //define new distance
            if (lastDistance > minDistance) //if distance is higher than maximum distance
            {
                newDistance = lastDistance + Random.Range(-randomRange, 0f); //randomise decrement
            }
            else if (lastDistance < maxDistance) //if distance is lower than minimum distance
            {

                newDistance = lastDistance + Random.Range(0f, randomRange); //randomize increment
            }
            else //else
            {
                newDistance = lastDistance + Random.Range(-randomRange, randomRange); //randomize change
            }

            GameObject bottomColumn = Instantiate(
                columnPrefab,
                new Vector3(i * columnPrefab.transform.localScale.x, newHeight, 0f),
                columnPrefab.transform.rotation); //spawn bottom object


            GameObject topcolumn = Instantiate(
                columnPrefab,
                bottomColumn.transform.position + Vector3.up * newDistance,
                columnPrefab.transform.rotation); //spawn top object

            if (i % 100 == 99) //once in every 10 blocks
            {
                phaseColor = new Color(
                    Random.Range(0.25f, 0.6f), 
                    Random.Range(0.25f, 0.6f), 
                    Random.Range(0.25f, 0.6f)); //change phase color

                GameObject portalWallObject = Instantiate(
                    portalWallPrefab,
                    bottomColumn.transform.position - 
                    Vector3.right * (bottomColumn.transform.localScale.x * 0.5f) + 
                    Vector3.up * (topcolumn.transform.position.y - 
                    bottomColumn.transform.position.y) * 0.5f,
                    portalWallPrefab.transform.rotation); //spawn portal wall

                portalWallObject.transform.localScale = 
                    new Vector3(0.01f, newDistance - (2f * bottomColumn.transform.localScale.y) * 0.5f, 0.75f); //set portal wall scale
                portalWallObject.transform.parent = gameObject.transform; //set parent to this

                portalWallObject.GetComponent<MeshRenderer>().material.color = phaseColor; //set portal wall material color

            }
            else if (i % 10 == 9) //once in every 10 blocks
            {
                GameObject circleObject = Instantiate(
                    circlePrefab,
                    bottomColumn.transform.position + Vector3.up * (topcolumn.transform.position.y - bottomColumn.transform.position.y) * 0.5f,
                    circlePrefab.transform.rotation); //spawn circle
                circleObject.transform.parent = gameObject.transform; //set parent to this
            }

            if (i % 2 == 1) //once in every 2 blocks
            {
                bottomColumn.GetComponent<MeshRenderer>().material.color = 
                    phaseColor; //set bottom column color
                topcolumn.GetComponent<MeshRenderer>().material.color = 
                    phaseColor; //set top column color
            }
            else //once in every 2 blocks
            {
                bottomColumn.GetComponent<MeshRenderer>().material.color = 
                    (phaseColor + Color.white * 0.5f) * 0.66f; //set bottom column a lighter color
                topcolumn.GetComponent<MeshRenderer>().material.color = 
                    (phaseColor + Color.white * 0.5f) * 0.66f;//set top column a lighter color
            }

            bottomColumn.transform.parent = gameObject.transform; //set parent to this
            topcolumn.transform.parent = gameObject.transform; //set parent to this

            lastHeight = newHeight; //update last height
            lastDistance = newDistance; //update last distance
        }

        yield return new WaitForEndOfFrame(); //wait for one frame
    }
}