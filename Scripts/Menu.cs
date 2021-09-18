using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class Menu : MonoBehaviourPunCallbacks, ILobbyCallbacks
{
    [Header("Screens")]
    public GameObject mainScreen;
    public GameObject createRoomScreen;
    public GameObject LobbyScreen;
    public GameObject LobbyBrowserScreen;

    [Header("Main Screen")]
    public Button createRoomButton;
    public Button findRoomButton;

    [Header("Lobby")]
    public TextMeshProUGUI playerListText;
    public TextMeshProUGUI roomInfoText;
    public Button startGameButton;

    [Header("Lobby Browser")]
    public RectTransform roomListContainer;
    public GameObject roomButtonPrefab;

    private List<GameObject> roomButtons = new List<GameObject>();
    private List<RoomInfo> roomList = new List<RoomInfo>();


    // Start is called before the first frame update
    void Start()
    {
        //dissable menue btn
        createRoomButton.interactable = false;
        findRoomButton.interactable = false;

        //turn on curser
        Cursor.lockState = CursorLockMode.None;

        //in game?
        if (PhotonNetwork.InRoom)
        {
            //go to lobby
            SetScreen(LobbyScreen);
            UpdateLobbyUI();


            //make room visable
            PhotonNetwork.CurrentRoom.IsVisible = true;
            PhotonNetwork.CurrentRoom.IsOpen = true;
        }
    }

    void SetScreen (GameObject screen)
    {
        //turn off screens
        mainScreen.SetActive(false);
        createRoomScreen.SetActive(false);
        LobbyScreen.SetActive(false);
        LobbyBrowserScreen.SetActive(false);

        //turn in requested screen
        screen.SetActive(true);

        if (screen == LobbyBrowserScreen)
            UpdateLobbyBrowserUI();
    }

    public void OnBackButton()
    {
        SetScreen(mainScreen);
    }

    //MAIN SCREEN

    public void OnPlayerNameValueChanged (TMP_InputField playerNameInput)
    {
        PhotonNetwork.NickName = playerNameInput.text; 
    }

    public override void OnConnectedToMaster()
    {
        //turn on buttons after connect to master
        createRoomButton.interactable = true;
        findRoomButton.interactable = true;

    }

    public void OnCreateRoomButton()
    {
        SetScreen(createRoomScreen);
    }

    public void OnFindRoomButton()
    {
        SetScreen(LobbyBrowserScreen);
    }

    //CREATE ROOM SCREEN

    public void OnCreateButton (TMP_InputField roomNameInput)
    {
        NetworkManager.instance.CreateRoom(roomNameInput.text);
    }

    //lobby screen

    public override void OnJoinedRoom()
    {
        SetScreen(LobbyScreen);
        photonView.RPC("UpdateLobbyUI", RpcTarget.All);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateLobbyUI();
    }

    [PunRPC]
    void UpdateLobbyUI()
    {
        //toggle start game button
        startGameButton.interactable = PhotonNetwork.IsMasterClient;

        //print all players
        playerListText.text = "";

        foreach (Player player in PhotonNetwork.PlayerList)
            playerListText.text += player.NickName + "\n";

        // set room info text
        roomInfoText.text = "<b>Room Name</b>\n" + PhotonNetwork.CurrentRoom.Name;

    }

    public void OnStartGameButton()
    {
        //hide room
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;

        //tell everyone to load into game
        NetworkManager.instance.photonView.RPC("ChangeScene", RpcTarget.All, "Game");
    }

    public void OnLeaveLobbyButton()
    {
        PhotonNetwork.LeaveRoom();
        SetScreen(mainScreen);
    }

    // LOBBY BROWSER SCREEN

    GameObject CreateRoomButton()
    {
        GameObject buttonObj = Instantiate(roomButtonPrefab, roomListContainer.transform);
        roomButtons.Add(buttonObj);

        return buttonObj;
    }

    void UpdateLobbyBrowserUI()
    {
        //disable all room buttons
        foreach (GameObject button in roomButtons)
            button.SetActive(false);

        //display all current rooms in the master server
        for (int x = 0; x < roomList.Count; ++x)
        {
            //get or create object
            GameObject button = x >= roomButtons.Count ? CreateRoomButton() : roomButtons[x];

            button.SetActive(true);

            //set the room name and player count text
            button.transform.Find("RoomInfoText").GetComponent<TextMeshProUGUI>().text = roomList[x].Name;
            button.transform.Find("PlayerCountText").GetComponent<TextMeshProUGUI>().text = roomList[x].PlayerCount + " / " + roomList[x].MaxPlayers;

            //set btn on click event
            Button buttonComp = button.GetComponent<Button>();

            string roomName = roomList[x].Name;

            buttonComp.onClick.RemoveAllListeners();
            buttonComp.onClick.AddListener(() => { OnJoinRoomButton(roomName); });
        }
    }

    public void OnJoinRoomButton (string roomName)
    {
        NetworkManager.instance.JoinRoom(roomName);
    }

    public void OnRefreshButton()
    {
        UpdateLobbyBrowserUI();
    }

    public override void OnRoomListUpdate(List<RoomInfo> allRooms)
    {
        roomList = allRooms;
    }
}
