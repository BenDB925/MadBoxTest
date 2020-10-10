using System.Collections;
using System.Collections.Generic;
using Obstacles;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameOverController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private Button retryButton;

    private Dictionary<int, List<string>> gameOverStrings;

    // Start is called before the first frame update
    void Start()
    {
        gameOverStrings = new Dictionary<int, List<string>> {
            {0, new List<string>()}, 
            {1, new List<string>()},
            {2, new List<string>()},
            {3, new List<string>()},
            {4, new List<string>()}
        };
        
        
        gameOverStrings[0].Add("NOT EVEN 1 LAP? GO AGAIN");
        gameOverStrings[0].Add("OOF - TRY AGAIN");
        gameOverStrings[1].Add("1 LAP? THATS ALL YOU HAVE? TRY AGAIN");
        gameOverStrings[1].Add("YOU'VE MADE A MESS OF THAT ONE, GIVE ME MORE THAN 1 LAP NEXT TIME");
        gameOverStrings[2].Add("2?");
        gameOverStrings[2].Add("2 LAPS IS PRETTY GOOD, DO I HEAR 3?");
        gameOverStrings[3].Add("3 LAPS, NOW WE'RE TALKING, CAN YOU BEAT IT?");
        gameOverStrings[3].Add("3 LAPS COMPLETED, I'M IMPRESSED");
        gameOverStrings[4].Add("WOW, THATS UP THERE WITH THE BEST OF THEM, CAN YOU PROVE IT AGAIN?");
        gameOverStrings[4].Add("YOU'RE GETTING GOOD AT THIS NOW, ONE MORE TRY?");
        
        
        PlayerController.deathEvent.AddListener(OnPlayerDeath);
        
        gameOverText.gameObject.SetActive(false);
        retryButton.gameObject.SetActive(false);
    }

    private void OnPlayerDeath()
    {
        gameOverText.gameObject.SetActive(true);
        retryButton.gameObject.SetActive(true);
        
        int lapIndex = Mathf.Clamp(GameInfoHolder.numLapsCompleted, 0, gameOverStrings.Count - 1);
        int textIndex = Random.Range(0, gameOverStrings[lapIndex].Count);
        gameOverText.text = gameOverStrings[lapIndex][textIndex];
    }

    public void HideGameOver()
    {
        gameOverText.gameObject.SetActive(false);
        retryButton.gameObject.SetActive(false);
    }
}
