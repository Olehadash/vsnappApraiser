using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerIOClient;
using System.Security.Cryptography;
using System;
using System.Text;

public class PlayerIOServerManager : MonoBehaviour
{
    #region Singleton
    private static PlayerIOServerManager instance;
    private static bool isNullInstance
    {
        get
        {
            if (instance == null)
            {
#if UNITY_EDITOR
                System.Diagnostics.StackFrame stackFrame = new System.Diagnostics.StackTrace(true).GetFrame(1);
                string scriptName = stackFrame.GetFileName();
                int lineNumber = stackFrame.GetFileLineNumber();
                Debug.LogError(scriptName + " instance not found at line " + lineNumber + " !");
#else
                Debug.LogError("PlayerIOServerManager instance not found!");
#endif
                return true;
            }
            return false;
        }
    }
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    #endregion

    #region Private Properties
    private bool isNullConnection
    {
        get
        {
            if (currentServerConnection == null)
            {
                string scriptName = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
                Debug.LogError("PlayerIOClient is not connected to Server!");
                return true;
            }
            return false;
        }
    }
    #endregion

    #region Private Fields
    private Client currentClient;
    private Connection currentServerConnection;

    private System.Action ConnectionSuccessEvent;
    private System.Action UserJoinedEvent;
    private System.Action LostConnectionEvent;
    private System.Action ConnectionFailedEvent;

    private Coroutine connectionWaitingCoroutine;

    private const string gameId = "vsnaap-ibl9rv4skw1zsa1eofma";

    private const string FirstConnectionMessageType = "FirstConnection";

    private const string GetCardsNamesMessageType = "GetCardsNames";
    private const string GetCardsDataMessageType = "GetCardsData";
    private const string GetCardsStatusMessageType = "GetCardsStatus";

    private const string GetMissedCallsDataMessageType = "GetMissedCallsData";
    private const string MissedCallMessageType = "MissedCall";
    private const string SendReadyForCallMessageType = "ReadyForCall";

    private const string GetPhotosDataMessageType = "GetPhotosData";
    private const string DeleteAppraiserDocumentMessageType = "DeleteAppraiserDocument";

    private const string CreateCardMessageType = "CreateCard";

    private const string SendSetPlayFabIDMessageType = "SetPlayFabID";
    private const string SendGetPlayFabIDMessageType = "GetPlayFabID";

    private const string ChangeCarNumberMessageType = "ChangeCarNumber";
    private const string ChangeCardNumberMessageType = "ChangeCardNumber";
    private const string CheckAvailableCardNumberMessageType = "CheckAvailableCardNumber";
    private const string DeleteChangedCardNumberMessageType = "DeleteChangedCardNumber";

    private const string AdditionalToCardMessageType = "AdditionalToCard";
    private const string CheckCardDataMessageType = "CheckCardData";
    private const string CloseCardMessageType = "CloseCard";

    private const string AddNewEmailMessageType = "AddNewEmail";
    private const string RemoveFromEmailsMessageType = "RemoveFromEmails";
    private const string GetGarageNamesMessageType = "GetGarageNames";
    private const string GetGarageDatasMessageType = "GetGarageDatas";

    private const string AddDocumentToGaragesMessageType = "AddDocumentToGarages";

    private const string SearchCardsMessageType = "SearchCards";

    private const string CheckingConnectionMessageType = "CheckingConnection";

    private const string DebugMessageType = "Debug";

    private string lastAppraiserPassword;
    #endregion

    #region Add Event Listiners
    public static void AddConnectionSuccessEventListiner(System.Action eventListiner)
    {
        if (isNullInstance)
            return;

        instance.ConnectionSuccessEvent += eventListiner;
    }

    public static void AddUserJoinedEventListiner(System.Action eventListiner)
    {
        if (isNullInstance)
            return;

        instance.UserJoinedEvent += eventListiner;
    }

    public static void AddLostConnectionEventListiner(System.Action eventListiner)
    {
        if (isNullInstance)
            return;

        instance.LostConnectionEvent += eventListiner;
    }

    public static void AddConnectionFailedEventListiner(System.Action eventListiner)
    {
        if (isNullInstance)
            return;

        instance.ConnectionFailedEvent += eventListiner;
    }
    #endregion

    #region Connection
    private IEnumerator WaitForConnection(string appraiserLogin, string appraiserPassword)
    {
        yield return null;

        while (true)
        {
            ConnectToServer(appraiserLogin, appraiserPassword);

            yield return new WaitForSeconds(7f);

            Debug.Log("Can't connect to Server... Retrying...");
        }
    }

    public static void ConnectToServer(string appraiserLogin, string appraiserPassword)
    {
        if (isNullInstance)
            return;

        if (instance.connectionWaitingCoroutine == null)
        {
            Debug.Log(string.Format("Player.io_: Start ConnectToServer with login: {0}, password: {1}",
                appraiserLogin, appraiserPassword));

            instance.connectionWaitingCoroutine =
                instance.StartCoroutine(instance.WaitForConnection(appraiserLogin, appraiserPassword));
            return;
        }
        else
        {
            Debug.Log(string.Format("Player.io_: Continue ConnectToServer with login: {0}, password: {1}",
                appraiserLogin, appraiserPassword));
        }

        if (instance.currentServerConnection != null)
        {
            Debug.LogError("Player.io_: Client is already connected to Server!");
            return;
        }
        Dictionary<string, string> authArgs = new Dictionary<string, string>();
        authArgs.Add("userId", appraiserLogin);
        instance.lastAppraiserPassword = appraiserPassword;

        PlayerIO.Authenticate(gameId, "public", authArgs, null,
            instance.ConnectionToServerSuccess,
            instance.ConnectionFailed);
    }
    #endregion

    #region Connection To Game Handler
    private void ConnectionToServerSuccess(Client client)
    {
        if (instance.currentClient != null)
            return;

        instance.currentClient = client;

        Dictionary<string, string> joinData = new Dictionary<string, string>();
        joinData.Add("password", lastAppraiserPassword);
        joinData.Add("accountType", "appraiser");

        client.Multiplayer.CreateJoinRoom(client.ConnectUserId, "RoomAppraiser", false, null, joinData,
            ConnectionToServerRoomSuccess,
            ConnectionFailed);
    }

    private void ConnectionFailed(PlayerIOError error)
    {
        Debug.LogError("Player.io_:  Server ConnectionFailed:\n" + error.Message);
    }
    #endregion

    #region Connection To Room Handler
    private void ConnectionToServerRoomSuccess(Connection connection)
    {
        Debug.Log("Player.io_: Connection to Room Success");
        currentServerConnection = connection;
        currentServerConnection.OnMessage += ServerMessageHandler;
        currentServerConnection.OnDisconnect += ServerDisconnectHandler;

        ConnectionSuccessEvent?.Invoke();

        if (connectionWaitingCoroutine != null)
        {
            StopCoroutine(connectionWaitingCoroutine);
            connectionWaitingCoroutine = null;
        }
    }
    #endregion

    #region Server Messages Part
    private void ServerMessageHandler(object sender, Message e)
    {
        switch (e.Type)
        {
            #region First Connection
            case FirstConnectionMessageType:
                {
                    bool result = e.GetBoolean(0);
                    if (result)
                    {
                        string rawMissedCallsData = e.GetString(1);
                        //HomeMissedCallListManager.SetupMissedCallsList(rawMissedCallsData);

                        string rawCardDatas = e.GetString(2);
                        //HomeCardListManager.SetupCardsList(rawCardDatas);

                        string rawDocumentsData = e.GetString(3);
                        //HomeDocumentsListManager.SetupDocumentsList(rawDocumentsData);

                        string rawEmailsData = e.GetString(4);
                        /*if (!string.IsNullOrEmpty(rawEmailsData))
                            EmailsListScreen.CreateEmailsList(rawEmailsData);*/

                        displayName = e.GetString(5);
                        //GameManager.SetUserDisplayName(displayName);

                        //if (checkConnectionCoroutine != null)
                        //{
                        //    Debug.Log("FirstConnectionMessageType warning: checkConnectionCoroutine is not null!");
                        //    StopCoroutine(checkConnectionCoroutine);
                        //    checkConnectionCoroutine = null;
                        //}
                        //checkConnectionCoroutine = StartCoroutine(CheckConnection());
                    }
                    else
                    {
                        string errorMessage = e.GetString(1);
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            Debug.Log("FirstConnection warning:\n" +
                                errorMessage);
                        }
                        else
                        {
                            Debug.Log("FirstConnection unknown warning...");
                        }
                        ConnectionFailedEvent?.Invoke();
                    }
                }
                break;
            #endregion

            #region Card Creation
            case CreateCardMessageType:
                {
                    bool result = e.GetBoolean(0);
                    /*if (result)
                        CardCreationScreen.CreationResult(result);
                    else
                        CardCreationScreen.CreationResult(result, e.GetString(1));*/
                }
                break;
            #endregion

            #region Cards Update
            case GetCardsNamesMessageType:
                {
                    bool result = e.GetBoolean(0);
                    if (result)
                    {
                        //HomeCardListManager.CheckUpdateCardsList(e.GetString(1).Split('_'));
                    }
                    else
                    {
                        string errorMessage = e.GetString(1);
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            Debug.LogError(GetCardsNamesMessageType + " error:\n" +
                                errorMessage);
                        }
                        else
                        {
                            Debug.LogError(GetCardsNamesMessageType + " unknown error...");
                        }
                    }
                }
                break;

            case GetCardsDataMessageType:
                {
                    bool result = e.GetBoolean(0);
                    if (result)
                    {
                        string rawCardDatas = e.GetString(1);
                        //HomeCardListManager.UpdateCardsList(rawCardDatas.Split('_'));
                    }
                    else
                    {
                        string errorMessage = e.GetString(1);
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            Debug.LogError(GetCardsDataMessageType + " error:\n" +
                                errorMessage);
                        }
                        else
                        {
                            Debug.LogError(GetCardsDataMessageType + " unknown error...");
                        }

                        if (e.Count < 2)
                        {
                            Debug.Log("GetBlocksData can't add 'problemCardNumber', cuz 'problemCardNumber' is null!");
                            return;
                        }

                        string problemCardNumber = e.GetString(2);
                        if (string.IsNullOrEmpty(problemCardNumber))
                        {
                            Debug.LogError("GetBlocksData 'problemBlockName' is null!");
                            return;
                        }
                        //HomeCardListManager.AddProblemCard(problemCardNumber);
                    }
                }
                break;
            #endregion

            #region Get Calls/Photos
            case GetMissedCallsDataMessageType:
                {
                    bool result = e.GetBoolean(0);
                    if (result)
                    {
                        //HomeMissedCallListManager.CheckUpdateMissedCallsList(e.GetString(1).Split('_'));
                    }
                    else
                    {
                        string errorMessage = e.GetString(1);
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            Debug.LogError(GetMissedCallsDataMessageType + " error:\n" +
                                errorMessage);
                        }
                        else
                        {
                            Debug.LogError(GetMissedCallsDataMessageType + " unknown error...");
                        }
                    }
                }
                break;

            case GetPhotosDataMessageType:
                {
                    bool result = e.GetBoolean(0);
                    if (result)
                    {
                        //HomeDocumentsListManager.CheckUpdateDocumentsList(e.GetString(1).Split('_'));
                    }
                    else
                    {
                        string errorMessage = e.GetString(1);
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            Debug.LogError(GetPhotosDataMessageType + " error:\n" +
                                errorMessage);
                        }
                        else
                        {
                            Debug.LogError(GetPhotosDataMessageType + " unknown error...");
                        }
                    }
                }
                break;
            #endregion

            #region Get Cards Status
            case GetCardsStatusMessageType:
                {
                    bool result = e.GetBoolean(0);
                    if (result)
                    {
                        string[] cardStatusData = e.GetString(1).Split('_');
                        //HomeCardListManager.UpdateStatusResult(cardStatusData);
                        //CardStatusScreen.UpdateStatusResult(cardStatusData);
                    }
                    else
                    {
                        string errorMessage = e.GetString(1);
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            Debug.LogError(GetCardsNamesMessageType + " error:\n" +
                                errorMessage);
                        }
                        else
                        {
                            Debug.LogError(GetCardsNamesMessageType + " unknown error...");
                        }
                    }
                }
                break;
            #endregion

            #region PlayFab ID Part
            case SendGetPlayFabIDMessageType:
                {
                    bool result = e.GetBoolean(0);
                    string message = e.GetString(1);
                    //CallingProcessManager.InviteGarageToRoom(result, message);
                    if (!result)
                    {
                        if (!string.IsNullOrEmpty(message))
                        {
                            Debug.LogError(SendGetPlayFabIDMessageType + " error:\n" +
                                message);
                        }
                        else
                        {
                            Debug.LogError(SendGetPlayFabIDMessageType + " unknown error...");
                        }
                    }
                }
                break;

            case SendSetPlayFabIDMessageType:
                {
                    bool result = e.GetBoolean(0);
                    if (!result)
                    {
                        string message = e.GetString(1);
                        if (!string.IsNullOrEmpty(message))
                        {
                            Debug.LogError(SendSetPlayFabIDMessageType + " error:\n" +
                                message);
                        }
                        else
                        {
                            Debug.LogError(SendSetPlayFabIDMessageType + " unknown error...");
                        }
                    }
                }
                break;
            #endregion

            #region Check Connection
            case CheckingConnectionMessageType:
                {
                    currentServerConnection.Send(CheckingConnectionMessageType, true);

                    SendGetMissedCallsDataMessage();
                    SendGetDocumentsDataMessage();

                    SendGetCardsNamesMessage();

                    SendGetGarageNamesMessage();

                    //HomeCardListManager.GetUpdateStatus();

                    //connectionChecked = true;
                }
                break;
            #endregion

            #region Change Card/Car Number
            case ChangeCarNumberMessageType:
                {
                    bool result = e.GetBoolean(0);
                    if (result)
                    {
                        string cardNumber = e.GetString(1);
                        string newCarNumber = e.GetString(2);
                        /*CardCheckingScreen.ChangeCarNumber(newCarNumber);
                        CardAdditionalScreen.ChangeCarNumber(newCarNumber);
                        HomeCardListManager.RemoveCardFromList(cardNumber);*/
                    }
                    else
                    {
                        string errorMessage = e.GetString(1);
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            Debug.LogError(ChangeCarNumberMessageType + " error:\n" +
                                errorMessage);
                        }
                        else
                        {
                            Debug.LogError(ChangeCarNumberMessageType + " unknown error...");
                        }
                    }
                }
                break;

            case CheckAvailableCardNumberMessageType:
                {
                    bool result = e.GetBoolean(0);
                    if (result)
                    {
                        string newCardNumber = e.GetString(1);
                        //string oldCardNumber = CardCheckingScreen.GetCardNumber();
                        //SendChangeCardNumber(oldCardNumber, newCardNumber);
                    }
                    else
                    {
                        bool isAvailableConflict = e.GetBoolean(1);
                        if (isAvailableConflict)
                        {
                            //CardAdditionalScreen.ShowAvailableConflictMessage();
                        }

                        string errorMessage = e.GetString(2);
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            Debug.LogError(CheckAvailableCardNumberMessageType + " error:\n" +
                                errorMessage);
                        }
                        else
                        {
                            Debug.LogError(CheckAvailableCardNumberMessageType + " unknown error...");
                        }
                    }
                }
                break;

            case ChangeCardNumberMessageType:
                {
                    bool result = e.GetBoolean(0);
                    if (result)
                    {
                        /*string newCardNumber = e.GetString(1);
                        string oldCardNumber = CardCheckingScreen.GetCardNumber();
                        string garageName = CardCheckingScreen.GetGarageName();*/

                        /**Debug.Log(string.Format("ChangeCardNumber {0} on {1} Success!",
                            oldCardNumber, newCardNumber));*/

                       /* CardCheckingScreen.ChangeCardNumber(newCardNumber);
                        CardAdditionalScreen.ChangeCardNumber(newCardNumber);
                        SendDeleteOldCardNumber(oldCardNumber, garageName);
                        HomeCardListManager.RemoveCardFromList(oldCardNumber);
                        HomeCardListManager.SkipCheckUpdate();*/
                    }
                    else
                    {
                        string errorMessage = e.GetString(1);
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            Debug.LogError(ChangeCardNumberMessageType + " error:\n" +
                                errorMessage);
                        }
                        else
                        {
                            Debug.LogError(ChangeCardNumberMessageType + " unknown error...");
                        }
                    }
                }
                break;

            case DeleteChangedCardNumberMessageType:
                {
                    bool result = e.GetBoolean(0);
                    if (result)
                    {
                        string oldCardNumber = e.GetString(1);
                        /*HomeCardListManager.RemoveCardFromList(oldCardNumber);*/
                    }
                    else
                    {
                        string errorMessage = e.GetString(1);
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            Debug.LogError(DeleteChangedCardNumberMessageType + " error:\n" +
                                errorMessage);
                        }
                        else
                        {
                            Debug.LogError(DeleteChangedCardNumberMessageType + " unknown error...");
                        }
                    }
                }
                break;
            #endregion

            #region Delete Appraiser Document
            case DeleteAppraiserDocumentMessageType:
                {
                    bool result = e.GetBoolean(0);
                    //AppraiserDocumentsScreen.UnlockBusy();
                    if (result)
                    {
                        string documentUrl = e.GetString(1);
                        //HomeDocumentsListManager.RemoveDocumentFromList(documentUrl);
                    }
                    else
                    {
                        string messageData = e.GetString(1);
                        if (!string.IsNullOrEmpty(messageData))
                        {
                            Debug.LogError(DeleteAppraiserDocumentMessageType + " error:\n" +
                                messageData);
                        }
                        else
                        {
                            Debug.LogError(DeleteAppraiserDocumentMessageType + " unknown error...");
                        }
                    }
                }
                break;
            #endregion

            #region Check/Additional/Close Card 
            case CheckCardDataMessageType:
                {
                    bool result = e.GetBoolean(0);
                    //CardCheckingScreen.CheckCardDataResult(result);
                    if (!result)
                    {
                        string messageData = e.GetString(1);
                        if (!string.IsNullOrEmpty(messageData))
                        {
                            Debug.Log(CheckCardDataMessageType + " error:\n" +
                                messageData);
                        }
                        else
                        {
                            Debug.Log(CheckCardDataMessageType + " unknown error...");
                        }
                    }
                }
                break;

            case AdditionalToCardMessageType:
                {
                    bool result = e.GetBoolean(0);
                    /*if (!CardAdditionalScreen.SendDocumentUrlsResult(result))
                    {
                        if (!AppraiserDocumentsScreen.AddDocumentToGaragesResult(result))
                            ResultCardCheckingScreen.SendDocumentUrlsResult(result); // Debbugable
                    }*/

                    //AppraiserDocumentsScreen.UnlockBusy();
                    if (!result)
                    {
                        string errorMessage = e.GetString(1);
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            Debug.LogError(AdditionalToCardMessageType + " error:\n" +
                                errorMessage);
                        }
                        else
                        {
                            Debug.LogError(AdditionalToCardMessageType + " unknown error...");
                        }
                    }
                }
                break;

            case CloseCardMessageType:
                {
                    bool result = e.GetBoolean(0);
                    if (result)
                    {
                        string cardNumber = e.GetString(1);
                        //HomeCardListManager.ChangeCardStatus(cardNumber);
                        //CardStatusScreen.CloseCardStatusResult(cardNumber, true);
                    }
                    else
                    {
                        string messageData = e.GetString(1);
                        if (!string.IsNullOrEmpty(messageData))
                        {
                            Debug.LogError(CloseCardMessageType + " error:\n" +
                                messageData);
                        }
                        else
                        {
                            Debug.LogError(CloseCardMessageType + " unknown error...");
                        }
                    }
                }
                break;
            #endregion

            #region Update Garage List
            case GetGarageNamesMessageType:
                {
                    bool result = e.GetBoolean(0);
                    if (result)
                    {
                        string rawAppraiserNames = e.GetString(1);
                        //GaragesListScreen.CheckUpdateGarageList(rawAppraiserNames);
                    }
                    else
                    {
                        string errorMessage = e.GetString(1);
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            Debug.LogError(GetGarageNamesMessageType + " error:\n" +
                                errorMessage);
                        }
                        else
                        {
                            Debug.LogError(GetGarageNamesMessageType + " unknown error...");
                        }
                    }
                }
                break;

            case GetGarageDatasMessageType:
                {
                    bool result = e.GetBoolean(0);
                    if (result)
                    {
                        string rawCardsData = e.GetString(1);
                        //GaragesListScreen.AddNewGaragesToList(rawCardsData.Split('_'));
                    }
                    else
                    {
                        string errorMessage = e.GetString(1);
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            Debug.LogError(GetGarageDatasMessageType + " error:\n" +
                                errorMessage);
                        }
                        else
                        {
                            Debug.LogError(GetGarageDatasMessageType + " unknown error...");
                        }
                    }
                }
                break;
            #endregion

            #region Add/Remove Email
            #region Add New Email
            case AddNewEmailMessageType:
                {
                    bool result = e.GetBoolean(0);
                    if (result)
                    {
                        string rawEmailData = e.GetString(1);
                        //EmailsListScreen.AddNewEmailSuccess(rawEmailData);
                    }
                    else
                    {
                        string errorMessage = e.GetString(1);
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            Debug.LogError(AddNewEmailMessageType + " error:\n" +
                                errorMessage);
                        }
                        else
                        {
                            Debug.LogError(AddNewEmailMessageType + " unknown error...");
                        }
                    }
                }
                break;
            #endregion

            #region Remove Email
            case RemoveFromEmailsMessageType:
                {
                    bool result = e.GetBoolean(0);
                    if (!result)
                    {
                        string errorMessage = e.GetString(1);
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            Debug.LogError(RemoveFromEmailsMessageType + " error:\n" +
                                errorMessage);
                        }
                        else
                        {
                            Debug.LogError(RemoveFromEmailsMessageType + " unknown error...");
                        }
                    }
                }
                break;
            #endregion
            #endregion

            #region Document To Garages
            case AddDocumentToGaragesMessageType:
                {
                    bool result = e.GetBoolean(0);
                    //GaragesListScreen.AddDocumentToGaragesResult(result);
                    if (!result)
                    {
                        string messageData = e.GetString(1);
                        if (!string.IsNullOrEmpty(messageData))
                        {
                            Debug.LogError(AddDocumentToGaragesMessageType + " error:\n" +
                                messageData);
                        }
                        else
                        {
                            Debug.LogError(AddDocumentToGaragesMessageType + " unknown error...");
                        }
                    }
                }
                break;
            #endregion

            #region Search Cards Result
            case SearchCardsMessageType:
                {
                    bool result = e.GetBoolean(0);
                    //CardSearchScreen.UnlockBusy();
                    if (result)
                    {
                        string rawSearchResultData = e.GetString(1);
                        //SearchResultScreen.ShowSearchResultScreen(rawSearchResultData);
                    }
                    else
                    {
                        string errorMessage = e.GetString(1);
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            Debug.Log(DebugMessageType + " message:\n" +
                                errorMessage);
                        }
                        else
                        {
                            Debug.LogError(DebugMessageType + " unknown error...");
                        }
                    }
                }
                break;
            #endregion

            #region Debug
            case DebugMessageType:
                {
                    string messageData = e.GetString(0);
                    if (!string.IsNullOrEmpty(messageData))
                    {
                        Debug.Log(DebugMessageType + " warning:\n" +
                            messageData);
                    }
                    else
                    {
                        Debug.Log(DebugMessageType + " unknown warning...");
                    }
                }
                break;
                #endregion
        }
    }

    #region Send Messages

    #region Search Cards
    public static void SendSearchCardsMessage(string rawSearchFilter)
    {
        if (isNullInstance || instance.isNullConnection)
            return;

        instance.currentServerConnection.Send(SearchCardsMessageType, rawSearchFilter);
    }
    #endregion

    #region Add Document To Garages
    public static void SendDocumentToGaragesMessage(string rawGarageNames, string rawDocumentData)
    {
        if (isNullInstance || instance.isNullConnection)
            return;

        instance.currentServerConnection.Send(AddDocumentToGaragesMessageType, rawGarageNames, rawDocumentData);
    }
    #endregion

    #region Add/Remove Email
    #region Add Email ToList
    public static void SendAddNewEmailMessage(string rawEmailData)
    {
        if (isNullInstance || instance.isNullConnection)
            return;

        instance.currentServerConnection.Send(AddNewEmailMessageType, rawEmailData);
    }
    #endregion

    #region Remove Email From List
    public static void SendRemoveFromEmailMessage(string rawEmailData)
    {
        if (isNullInstance || instance.isNullConnection)
            return;

        instance.currentServerConnection.Send(RemoveFromEmailsMessageType, rawEmailData);
    }
    #endregion
    #endregion


    #region Get Garages List
    private static void SendGetGarageNamesMessage()
    {
        if (isNullInstance || instance.isNullConnection)
            return;

        instance.currentServerConnection.Send(GetGarageNamesMessageType, true);
    }

    public static void SendGetGarageDatasMessage(string garageNames)
    {
        if (isNullInstance || instance.isNullConnection)
            return;

        instance.currentServerConnection.Send(GetGarageDatasMessageType, garageNames);
    }
    #endregion

    #region Get Cards Status
    public static void SendGetCardsStatusMessage(string rawCardNames)
    {
        if (isNullInstance || instance.isNullConnection)
            return;

        instance.currentServerConnection.Send(GetCardsStatusMessageType, rawCardNames);
    }
    #endregion

    #region Additional/Check/Close Card
    public static void SendAdditionalToCard(string rawAdditionalData)
    {
        if (isNullInstance || instance.isNullConnection)
            return;

        instance.currentServerConnection.Send(AdditionalToCardMessageType, rawAdditionalData);
    }

    public static void SendCheckCardDataMessage(string cardData)
    {
        if (isNullInstance || instance.isNullConnection)
            return;

        instance.currentServerConnection.Send(CheckCardDataMessageType, cardData);
    }

    public static void SendCloseCard(string rawClosedCardData)
    {
        if (isNullInstance || instance.isNullConnection)
            return;

        instance.currentServerConnection.Send(CloseCardMessageType, rawClosedCardData);
    }
    #endregion

    #region Delete Appraiser Document
    public static void SendDeleteAppraiserDocumentMessage(string rawDocumentData)
    {
        if (isNullInstance || instance.isNullConnection)
            return;

        instance.currentServerConnection.Send(DeleteAppraiserDocumentMessageType, rawDocumentData);
    }
    #endregion

    #region Change Car/Card Numbers

    #region Card Part
    private static void SendDeleteOldCardNumber(string oldCardNumber, string garageName)
    {
        if (isNullInstance || instance.isNullConnection)
            return;

        instance.currentServerConnection.Send(DeleteChangedCardNumberMessageType, oldCardNumber, garageName);
    }
    private static void SendChangeCardNumber(string oldCardNumber, string newCardNumber)
    {
        if (isNullInstance || instance.isNullConnection)
            return;

        instance.currentServerConnection.Send(ChangeCardNumberMessageType,
            oldCardNumber, newCardNumber);
    }
    public static void SendCheckAvailableCardNumber(string newCardNumber)
    {
        if (isNullInstance || instance.isNullConnection)
            return;

        instance.currentServerConnection.Send(CheckAvailableCardNumberMessageType, newCardNumber);
    }
    #endregion

    #region Car Part
    public static void SendChangeCarNumber(string cardNumber, string carNumber)
    {
        if (isNullInstance || instance.isNullConnection)
            return;

        instance.currentServerConnection.Send(ChangeCarNumberMessageType,
            cardNumber, carNumber);
    }
    #endregion

    #endregion

    #region Ready For Call
    public static void SendReadyForCallMessage(string missedCallData)
    {
        if (isNullInstance || instance.isNullConnection)
            return;

        instance.currentServerConnection.Send(SendReadyForCallMessageType, missedCallData);
    }
    #endregion

    #region Missed Call
    public static void SendMissedCallMessage(string missedCallData)
    {
        if (isNullInstance || instance.isNullConnection)
            return;

        instance.currentServerConnection.Send(MissedCallMessageType, missedCallData);
    }
    #endregion

    #region Get Update Status
    private static void SendGetCardsNamesMessage()
    {
        if (isNullInstance || instance.isNullConnection)
            return;

        instance.currentServerConnection.Send(GetCardsNamesMessageType, true);
    }
    #endregion

    #region Get Data
    public static void SendGetCardsDataMessage(string rawDataNames)
    {
        if (isNullInstance || instance.isNullConnection)
            return;

        instance.currentServerConnection.Send(GetCardsDataMessageType, rawDataNames);
    }
    public static void SendGetMissedCallsDataMessage()
    {
        if (isNullInstance || instance.isNullConnection)
            return;

        instance.currentServerConnection.Send(GetMissedCallsDataMessageType, true);
    }

    public static void SendGetDocumentsDataMessage()
    {
        if (isNullInstance || instance.isNullConnection)
            return;

        instance.currentServerConnection.Send(GetPhotosDataMessageType, true);
    }
    #endregion

    #region Create Card
    public static void SendCreateCardMessage(string rawCardData)
    {
        if (isNullInstance || instance.isNullConnection)
            return;

        instance.currentServerConnection.Send(CreateCardMessageType, rawCardData);
    }
    #endregion

    #region Get PlayFab ID
    public static void SendGetPlayFabID(string garageName)
    {
        if (isNullInstance || instance.isNullConnection)
            return;

        instance.currentServerConnection.Send(SendGetPlayFabIDMessageType, garageName);
    }
    #endregion

    #region SetPlayFabID
    public static void SendSetPlayFabID(string playFabID)
    {
        if (isNullInstance || instance.isNullConnection)
            return;

        instance.currentServerConnection.Send(SendSetPlayFabIDMessageType, playFabID);
    }
    #endregion

    #endregion

    #endregion

    #region Check Connection
    //private Coroutine checkConnectionCoroutine;
    //private bool connectionChecked;
    //private IEnumerator CheckConnection()
    //{
    //    while (true)
    //    {
    //        yield return new WaitForSecondsRealtime(10f);
    //        if (!connectionChecked)
    //            ManualDisconnect();
    //        connectionChecked = false;
    //    }
    //}
    #endregion

    #region Disconnect Part
    public static void StopConnectionToServer()
    {
        if (isNullInstance)
            return;

        Debug.Log("StopConnectionToServer");
        if (instance.connectionWaitingCoroutine != null)
        {
            Debug.Log("Stop connectionWaitingCoroutine...");
            instance.StopCoroutine(instance.connectionWaitingCoroutine);
            instance.connectionWaitingCoroutine = null;
        }
        else
        {
            instance.ManualDisconnect();
        }
    }
    private void ManualDisconnect()
    {
        if (isNullInstance)
            return;

        if (currentServerConnection != null)
        {
            Debug.Log("Server ManualDisconnect");

            instance.currentServerConnection.Disconnect();
            currentServerConnection = null;
            currentClient = null;
        }
    }
    private void ServerDisconnectHandler(object sender, string message)
    {
        Debug.Log(string.Format("ServerDisconnectHandler." +
            "Client was disconnected by {0} with reason:\n{1}", sender.ToString(), message));

        if (connectionWaitingCoroutine != null)
        {
            Debug.Log("Stop connectionWaitingCoroutine");

            StopCoroutine(connectionWaitingCoroutine);
            connectionWaitingCoroutine = null;
        }

        if (currentServerConnection != null)
        {
            currentClient = null;
            currentServerConnection = null;
            LostConnectionEvent?.Invoke();
        }

        //if (checkConnectionCoroutine != null)
        //{
        //    Debug.Log("Stop checkConnectionCoroutine");

        //    StopCoroutine(checkConnectionCoroutine);
        //    checkConnectionCoroutine = null;
        //}
    }

    private void OnApplicationQuit()
    {
        if (currentServerConnection != null)
        {
            currentServerConnection.Disconnect();
            currentServerConnection = null;
            currentClient = null;
        }
    }
    #endregion

    public string displayName = "";
    public static string DisplayName
    {
        get {
            if (isNullInstance) return "";
            return instance.displayName;
                }
    }
}
