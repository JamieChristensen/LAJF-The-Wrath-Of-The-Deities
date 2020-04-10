﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using STL2.Events;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ChooseBetweenOptionsGiven : MonoBehaviour
{
    #region INSPECTOR
    CustomSceneManager customSceneManager;
    public RuntimeChoiceManager runtimeChoiceManager;
    public VoidEvent godsHaveChosenMinion;
    public VoidEvent godsHaveChosenOpponent;
    public VoidEvent heroHasChosenItem;
    public VoidEvent heroWon;

    public IntEvent preGameTransitionIndex;

    public ChoiceCategory character;
    public ChoiceCategory theme;
    public ChoiceCategory minion;
    public ChoiceCategory modifier;
  //  public PlayerItems item;
   // public EnemyModifier enemyModifier;
    public string choiceType;


    public ChoiceCategory type;
    public ChoiceCategory runtimeChoices;

    public GameObject finalChoice; // the gameObject that has been chosen

    private int choice = 4; // this is 4 by default as there never will be 4 choices to choose from. The forth choice is blindpick.

    [Header("Character choice variables")]
    public CharacterPool characterpool;
    public List<Image> characterSprites;
    public List<TextMeshProUGUI> characterNames;
    private P1Stats[] characterChoices = new P1Stats[3];

    [Header("Item choice variables")]
    public PlayerItems[] playerItemPool;
    public TextMeshProUGUI[] choiceNameText;
    public Image[] itemImageTargets;
    private PlayerItems[] playerItemChoices = new PlayerItems[2];
    public GameObject theOnlyButton;
    public Sprite theGrandPrize;

    [Header("Modifier choice variables")]
    public EnemyModifier[] modifierPool;
    public List<Image>  modifierSprites;
    public List<TextMeshProUGUI> modifierNames;
    private EnemyModifier[] enemyModifierChoices = new EnemyModifier[3];

    [Header("Minion choice variables")]
    public Enemy[] enemyPool;
    public List<Image> enemySprites;
    public List<TextMeshProUGUI> enemyNames;
    private Enemy[] enemyChoices = new Enemy[3];

    [Header("Theme choice variables")]
    public EnvironmentPool environmentThemePool;
    public List<Image> themeSprites;
    public List<TextMeshProUGUI> themeNames;
    public Environment[] environmentThemeChoices = new Environment[3];
    public List<Environment> EnvironmentsChosen; // the individual environments chosen

    private MusicManager _musicManager;


    #endregion // INSPECTOR

    private void Awake()
    {
        _musicManager = FindObjectOfType<MusicManager>();

        choice = 4; // the choice is set back to the default value
        try
        {
            customSceneManager = GameObject.Find("SceneManager").GetComponent<CustomSceneManager>();
        }
        catch
        {

        }

        if (choiceType == "Character")
        {
            #region InitializeCharacterSelection
            ShuffleList(characterpool.characterStats);
            for (int i = 0; i < characterChoices.Length; i++)
            {
                P1Stats characterStats = characterpool.characterStats[i];
                characterSprites[i].sprite = characterStats.characterSprite;
                characterNames[i].text = characterStats.myName;
                characterChoices[i] = characterStats;
            }
            #endregion InitializeCharacterSelection
        }

        if (choiceType == "Item")
        {
            #region InitializeItemSelection
            if (runtimeChoices.runTimeLoopCount != 4)
            {
                int random = Random.Range(0, playerItemPool.Length);
                int random2 = random;
                if (playerItemPool.Length <= 1)
                {
                    random2 = 3;
                }

                while (random2 == random)
                {
                    Debug.Log("random2 was the same as random, rerolling");
                    random2 = Random.Range(0, playerItemPool.Length);
                }
                if (runtimeChoices.playerItems.Count != 2)
                {
                   // runtimeChoices.playerItems.Capacity = 2;
                }
                playerItemChoices[0] = playerItemPool[random];
                playerItemChoices[1] = playerItemPool[random2];
                choiceNameText[0].text = playerItemChoices[0].name;
                choiceNameText[1].text = playerItemChoices[1].name;
                itemImageTargets[0].sprite = playerItemChoices[0].itemSprite;
                itemImageTargets[1].sprite = playerItemChoices[1].itemSprite;
            }
            else
            {
                itemImageTargets[0].gameObject.SetActive(false);
                itemImageTargets[1].gameObject.SetActive(false);
                theOnlyButton.SetActive(true);
                theOnlyButton.GetComponent<Image>().sprite = theGrandPrize;
                theOnlyButton.GetComponentInChildren<TextMeshProUGUI>().text = "Sweet Victory";
            }
           
            #endregion InitializeItemSelection
        }

        if (choiceType == "Minion") // naming inconsistency between Minion and Enemy
        {
            #region InitializeMinionSelection
            ShuffleList(enemyPool);
             for (int i = 0; i < enemyModifierChoices.Length; i++)
             {
                 Enemy enemy = enemyPool[i];
                 enemySprites[i].sprite = enemy.sprite;
                 enemyNames[i].text = enemy.name;
                 enemyChoices[i] = enemy;
             }
            #endregion InitializeMinionSelection
        }

        if (choiceType == "Modifier")
        {
            #region InitializeModifierSelection
            ShuffleList(modifierPool);
            for (int i = 0; i < enemyModifierChoices.Length; i++)
            {
                EnemyModifier modifier = modifierPool[i];
                modifierSprites[i].sprite = modifier.sprite;
                modifierNames[i].text = modifier.name;
                enemyModifierChoices[i] = modifier;
            }
            #endregion InitializeModifierSelection

        }

        if (choiceType == "Theme")
        {
            #region InitializeThemeSelection

            ShuffleList(environmentThemePool.environmentPool);
            for (int i = 0; i < environmentThemeChoices.Length; i++)
            {
                Environments environmentsTheme = environmentThemePool.environmentPool[i];
                themeSprites[i].sprite = environmentsTheme.environments[0].environmentSprite;
                themeNames[i].text = environmentsTheme.environments[0].environmentName;
                environmentThemeChoices[i] = environmentsTheme.environments[0];
            }

            #endregion InitializeThemeSelection
        }

    }

    private void Start()
    {
        int buildIndex = SceneManager.GetActiveScene().buildIndex;
        if (buildIndex == 1 || buildIndex == 2 || buildIndex == 3)
        {
            preGameTransitionIndex.Raise(2 * buildIndex - 1);
        }




    }


    #region choosing

    public void ChooseCharacter(int characterIndex)
    {
        choice = characterIndex;
    }

    public void ChooseTheme(int themeIndex)
    {
        choice = themeIndex;
    }

    public void ChooseMinion(int minionIndex)
    {
        choice = minionIndex;
    }

    public void ChooseModifier(int modifierIndex)
    {
        choice = modifierIndex;
    }

    public void ChooseItem(int itemIndex)
    {
        choice = itemIndex;
    }
    #endregion // choosing


    #region LockingChoice
    public void LockSelectedChoice(string choiceType)
    {

        ConvertChoiceToGameObject(choice, choiceType);

        //  RaiseEvent(choiceType);
    }

    void ConvertChoiceToGameObject(int choice, string choiceType)
    {
        int indexMaximum; // how many things can you choose from

        switch (choiceType)
        {
            case "Item":
                //type = item;
                indexMaximum = 2;
                break;

            case "Character":
                type = character;
                goto default;

            case "Minion":
                type = minion;
                goto default;

            case "Modifier":
                type = modifier;
                goto default;

            case "Theme":
                type = theme;
                goto default;

            default: // Character, Minion, Modifier, Theme

                indexMaximum = 3;
                break;
        }

        switch (choice)
        {
            case 1:
                finalChoice = type.Options[choice - 1];
                break;

            case 2:
                finalChoice = type.Options[choice - 1];
                break;

            case 3:

                if (indexMaximum == 3)
                {
                    finalChoice = type.Options[choice - 1];
                }
                else
                {
                    goto default;
                }

                break;

            default:

                choice = Random.Range(1, indexMaximum + 1);
                finalChoice = type.Options[choice - 1];
                this.choice = choice;
                break;
        }

        RaiseEvent(choiceType);
    }

    #endregion // LockingChoice



    #region RaisingEvents

    void RaiseEvent(string choiceType)
    {

        switch (choiceType)
        {
            case "Item":
                HeroHasChosenItem();
                break;

            case "Character":
                HeroHasChosenCharacter();
                break;

            case "Minion":
                GodsHaveChosenMinion();
                break;

            case "Modifier":
                GodsHaveChosenOpponent();
                break;

            case "Theme":
                GodsHaveChosenTheme();
                break;

            default:

                break;
        }

    }


    void HeroHasChosenCharacter()
    {
       // runtimeChoices.character = finalChoice;
        runtimeChoices.chosenHero = characterChoices[choice - 1];
        Debug.Log("Hero has chosen a character! It is: " + characterChoices[choice - 1].myName);
        SwitchToGodSelection(); // switching from character select to theme select
    }

    void GodsHaveChosenTheme()
    {
       EnvironmentsChosen.Add(environmentThemeChoices[choice - 1]); // the chosen Environment theme is the initilized

        for (int i = 0; i < environmentThemePool.environmentPool.Count; i++) // the Environment Theme Pool is checked
        {
            if (EnvironmentsChosen[0].theme == environmentThemePool.environmentPool[i].theme) // if the chosen theme matches with a theme in the Environment Theme Pool
            {
                EnvironmentsChosen = new List<Environment>();

                for (int ii = 1; ii < environmentThemePool.environmentPool[i].environments.Length;ii++)
                {
                    EnvironmentsChosen.Add(environmentThemePool.environmentPool[i].environments[ii]); // locates all the possible Environments in the chosen theme
                }
            }
        }

        ShuffleList(EnvironmentsChosen); // randomise the order
        /* choose 4 environments */
        for (int i = 0; i < 4; i++) 
        {
            runtimeChoices.chosenEnvironments[i] = EnvironmentsChosen[i];
        }
        Debug.Log("Gods have chosen a theme! It is: " + finalChoice.name);
        preGameTransitionIndex.Raise(6);
    }

    void GodsHaveChosenMinion()
    {
        switch (runtimeChoices.runTimeLoopCount)
        {
            case 1:
                runtimeChoices.firstOpponent.minion = finalChoice;
                break;
            case 2:
                runtimeChoices.secondOpponent.minion = finalChoice;
                break;
            case 3:
                runtimeChoices.thirdOpponent.minion = finalChoice;
                break;
            case 4:
                runtimeChoices.fourthOpponent.minion = finalChoice;
                break;
        }
        // Debug.Log("Gods have chosen the " + runtimeChoices.runTimeLoopCount + ". minion! It is: " + finalChoice.name);
        
            runtimeChoices.enemies.Add(enemyChoices[choice - 1]); // adds the chosen minion to the array
            SwitchToModifierSelection(); // switching from minion select to modifier select

    }


    void GodsHaveChosenOpponent() // this is raised in the choose modifier scene
    {
        switch (runtimeChoices.runTimeLoopCount)
        {
            case 1:
                runtimeChoices.firstOpponent.modifier = finalChoice;
                break;
            case 2:
                runtimeChoices.secondOpponent.modifier = finalChoice;
                break;
            case 3:
                runtimeChoices.thirdOpponent.modifier = finalChoice;
                break;
            case 4:
                runtimeChoices.fourthOpponent.modifier = finalChoice;
                break;
        }
        //  Debug.Log("Gods have chosen the " + runtimeChoices.runTimeLoopCount + ". modifier! It is: " + finalChoice.name);
        if (_musicManager != null)
        {
            _musicManager.PlayMusic("Battle");
        }
            runtimeChoices.enemyModifiers.Add(enemyModifierChoices[choice - 1]); // adds the chosen modifier to the array
            godsHaveChosenOpponent.Raise(); // Raising event for opponent chosen
    }

    void HeroHasChosenItem()
    {
        switch (runtimeChoices.runTimeLoopCount)
        {
            case 1:
                runtimeChoices.firstItem = finalChoice;
                break;
            case 2:
                runtimeChoices.secondItem = finalChoice;
                break;
            case 3:
                runtimeChoices.thirdItem = finalChoice;
                break;
        }
        if (runtimeChoices.runTimeLoopCount != 4)
        {
            Debug.Log("Hero has chosen the" + runtimeChoices.runTimeLoopCount + ". item! It is: " + finalChoice.name);
            runtimeChoices.playerItems.Add(playerItemChoices[choice - 1]); // overwrites the old chosen item
            heroHasChosenItem.Raise(); // Raising event for item chosen
                                       //Destroy(gameObject);
        }
        else
        {
            Debug.Log("The Hero Won!");
            heroWon.Raise();
        }

    }


    #endregion // RaisingEvents


    public void SwitchToGodSelection() // from character selection
    {
        preGameTransitionIndex.Raise(2);

    }

    public void SwitchToModifierSelection() // from minion selection
    {

        godsHaveChosenMinion.Raise();

    }


    public void ResetAllChoices()
    {
        /* Local */
        type = null;
        finalChoice = null;
        choice = 4;

        /* in scriptableObject */

        // TODO: MOVE LOGIC TO SCRIPTABLE OBJECT

        runtimeChoiceManager.ResetRun();

    }



    #region PreGameLoadingScenes

    public void OnPreGameLoadNextScene() // Character & Theme Screen
    {
        //  Debug.Log("Going to next scene!");
        int buildIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadSceneAsync(buildIndex + 1); // go to next scene
    }

    #endregion


    #region ShuffleFunctionOverloads
    private void ShuffleList(EnemyModifier[] ts)
    {
        for (int i = 0; i < ts.Length; i++)
        {
            EnemyModifier temp = ts[i];
            int randomIndex = Random.Range(i, ts.Length);
            ts[i] = ts[randomIndex];
            ts[randomIndex] = temp;
        }
    }

    private void ShuffleList(Enemy[] ts)
    {
        for (int i = 0; i < ts.Length; i++)
        {
            Enemy temp = ts[i];
            int randomIndex = Random.Range(i, ts.Length);
            ts[i] = ts[randomIndex];
            ts[randomIndex] = temp;
        }
    }

    private void ShuffleList(List<Environments> ts)
    {
        for (int i = 0; i < ts.Count; i++)
        {
            Environments temp = ts[i];
            int randomIndex = Random.Range(i, ts.Count);
            ts[i] = ts[randomIndex];
            ts[randomIndex] = temp;
        }
    }

    private void ShuffleList(List<Environment> ts)
    {
        for (int i = 0; i < ts.Count; i++)
        {
            Environment temp = ts[i];
            int randomIndex = Random.Range(i, ts.Count);
            ts[i] = ts[randomIndex];
            ts[randomIndex] = temp;
        }
    }

    private void ShuffleList(List<P1Stats> ts)
    {
        for (int i = 0; i < ts.Count; i++)
        {
            P1Stats temp = ts[i];
            int randomIndex = Random.Range(i, ts.Count);
            ts[i] = ts[randomIndex];
            ts[randomIndex] = temp;
        }
    }
    #endregion ShuffleFunctionOverloads
}
