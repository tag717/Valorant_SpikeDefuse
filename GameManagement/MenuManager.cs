using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuManager : MonoBehaviour
{
    private string currentGame;

    public GameObject unratedImage;
    public GameObject rankedImage;
    public GameObject rangeImage;


    public void StartGame()
    {
        if (currentGame == "RANGE")
        {
            SceneManager.LoadScene("RangeLoadingScene"); // replace with your scene name
        }
        else
        {
            Debug.Log("Only range is available at current version");
        }
    }

    public void OnUnratedSelect()
    {
        unratedImage.SetActive(true);
        currentGame = "UNRATED";
    }
    public void OnRankedSelect()
    {
        rankedImage.SetActive(true);
        currentGame = "RANKED";
    }
    public void OnRangeSelect()
    {
        rangeImage.SetActive(true);
        currentGame = "RANGE";
    } 
    
}