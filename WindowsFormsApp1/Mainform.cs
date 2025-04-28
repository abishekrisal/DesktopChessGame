using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Dynamic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.LinkLabel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;

namespace WindowsFormsApp1
{
    public partial class ChessBoard : Form
    {
        public ChessBoard()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;

            // Assign chess pieces && board
            Array.Copy(_chessboardpieces, boardpieces, _chessboardpieces.Length);
        }

        // class member array of Panels to track chessboard tiles
        private Panel[,] _chessBoardPanels;

        const int tileSize = 70;
        const int gridSize = 8;

        // location where chess pieces are stored 
        readonly string ImageLocationPiece = "C:\\Users\\abhis\\source\\repos\\ChessProject\\WindowsFormsApp1\\chesspieces";

        //Chess layout
        readonly string[,] _chessboardpieces = new string[8, 8]
        {           { "black_rook", "black_knight", "black_bishop", "black_queen", "black_king", "black_bishop", "black_knight", "black_rook" },
                    { "black_pawn", "black_pawn", "black_pawn", "black_pawn", "black_pawn", "black_pawn", "black_pawn", "black_pawn" },
                    { "", "", "", "", "", "", "", "" },
                    { "", "", "", "", "", "", "", "" },
                    { "", "", "", "", "", "", "", "" },
                    { "", "", "", "", "", "", "", "" },
                    { "white_pawn", "white_pawn", "white_pawn", "white_pawn", "white_pawn", "white_pawn", "white_pawn", "white_pawn" },
                    { "white_rook", "white_knight", "white_bishop", "white_queen", "white_king", "white_bishop", "white_knight", "white_rook" }
        };

        // will start with white and change every turn white to black and vice-versa.
        string CurrentPlayer;

        public string[,] boardpieces = new string[8, 8];

        bool OkayToCastleWhite = true;
        bool OkayToCastleBlack = true;

        bool LastMoveWasCastleChange = false;
        bool LastMoveWasPawnPromotion = false;

        string GameWinner;

        bool blackKingIsInCheck = false;

        bool whiteKingIsInCheck = false;

        private Panel selectedFromPanel = null;
        private Panel selectedToPanel = null;

        List<int[]> PlayerPositionsToCheckCurrentPlayerKing = new List<int[]>();

        List<int[]> TempCheckForStaleMate = new List<int[]>();

        Color previousbackgroundcolor;

        // form load start the chessgame
        private void Mainform_Load(object sender, EventArgs e)
        {

            //Load chessboard && images
            LoadChessPeicesAndBoard();

            // first white player makes a move // can have names from user to display instead
            CurrentPlayer = "whiteplayer";
            txtPlayerTurn.Text = "whiteplayer";

        }

        //Start new game after winner/loser
        private void btnRestart_Click(object sender, EventArgs e)
        {
            RestartGame();
        }

        void RestartGame()
        {
            OkayToCastleWhite = true;
            OkayToCastleBlack = true;
            LastMoveWasCastleChange = false;
            blackKingIsInCheck = false;
            whiteKingIsInCheck = false;
            selectedFromPanel = null;
            selectedToPanel = null;
            GameWinner = "";
            PlayerPositionsToCheckCurrentPlayerKing.Clear();

            Array.Copy(_chessboardpieces, boardpieces, _chessboardpieces.Length);

            //Load chessboard && images
            LoadChessPeicesAndBoard();

            // first white player makes a move // can have names from user to display instead
            CurrentPlayer = "whiteplayer";
            txtPlayerTurn.Text = "whiteplayer";
        }

        // End of game lock all the chesssquares
        void EndOfGame()
        {
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    // Disable the square (Panel)
                    _chessBoardPanels[x, y].Enabled = false;

                    // Also disable any PictureBox (chess piece) inside
                    foreach (Control control in _chessBoardPanels[x, y].Controls)
                    {
                        if (control is PictureBox piece)
                        {
                            piece.Enabled = false;
                        }
                    }
                }
            }
        }

        //Load the chessboard, chessimages and also assign for onclick function
        void LoadChessPeicesAndBoard()
        {
            try
            {
                var clr1 = Color.DarkGray;
                var clr2 = Color.White;

                chessboardpanel.Controls.Clear(); // Clear old board if restarting
                chessboardpanel.Size = new Size(tileSize * gridSize, tileSize * gridSize); //size of the chess panel

                // initialize the "chess board"
                _chessBoardPanels = new Panel[gridSize, gridSize];

                for (int x = 0; x < gridSize; x++)
                {
                    for (int y = 0; y < gridSize; y++)
                    {
                        Panel newPanel = new Panel
                        {
                            Size = new Size(tileSize, tileSize),
                            Location = new Point(tileSize * x, tileSize * y),
                            BorderStyle = BorderStyle.FixedSingle,
                            Tag = new Point(x, y)
                        };

                        //click event handler
                        newPanel.Click += ChessBoard_Click;

                        // color the backgrounds
                        Color tileColor;
                        if ((x + y) % 2 == 0)
                        { newPanel.BackColor = clr1; tileColor = clr1; }
                        else { newPanel.BackColor = clr2; tileColor = clr2; }

                        // add to form and array
                        chessboardpanel.Controls.Add(newPanel);

                        _chessBoardPanels[x, y] = newPanel;

                        // Assign chess pieces
                        try
                        {
                            string pieceName = boardpieces[y, x];

                            if (!string.IsNullOrEmpty(pieceName))
                            {
                                PictureBox piece = new PictureBox
                                {
                                    Size = new Size(tileSize, tileSize),
                                    SizeMode = PictureBoxSizeMode.StretchImage,
                                    Image = Image.FromFile($"{ImageLocationPiece}/{pieceName}.png"),
                                    BorderStyle = BorderStyle.FixedSingle,
                                    Tag = new Point(x, y)
                                };

                                piece.Click += ChessBoard_Click;
                                newPanel.Controls.Add(piece);
                            }
                        }
                        catch (Exception e) { MessageBox.Show("Error in processing chess pieces" + e.Message); }
                    }
                }
            }
            catch (Exception e) { MessageBox.Show("Error creating chess board" + e.Message); }
        }

        // Whenever the there is click on the squares of chessboard, do something.
        // main logicflow is here.
        void ChessBoard_Click(object sender, EventArgs e)
        {
            Panel clickedPanel = sender as Panel;

            if (clickedPanel == null && sender is PictureBox)
                clickedPanel = ((PictureBox)sender).Parent as Panel;

            if (clickedPanel == null)
                return;

            if (selectedFromPanel == null)
            {
                selectedFromPanel = clickedPanel;
                Point from = (Point)selectedFromPanel.Tag;
                string pieceName = boardpieces[from.Y, from.X]; // Remember: [row, col]
                string currentplay = (CurrentPlayer == "blackplayer") ? "black" : "white";

                if ((pieceName != "") && (pieceName.Contains(currentplay)))
                {
                    // First check the player turn and assign the value
                    previousbackgroundcolor = selectedFromPanel.BackColor;
                    selectedFromPanel.BackColor = Color.LightGreen;
                }
                else
                {
                    selectedFromPanel = null;
                }
            }
            else if (selectedToPanel == null)
            {
                selectedToPanel = clickedPanel;

                // Retrieve grid coordinates from Tag
                Point from = (Point)selectedFromPanel.Tag;
                Point to = (Point)selectedToPanel.Tag;

                // Reset highlights
                selectedFromPanel.BackColor = previousbackgroundcolor;

                selectedFromPanel = null;
                selectedToPanel = null;

                // move logic here!
                try
                {
                    if (!(from == to))
                    {
                        string fromPos = $"{(from.Y)}{(from.X)}";
                        string toPos = $"{(to.Y)}{(to.X)}";
                        int[] digits;

                        // Get piece name from matrix (if you track it)
                        string pieceName = boardpieces[from.Y, from.X]; // Remember: [row, col]
                        string currentplay = (CurrentPlayer == "blackplayer") ? "black" : "white";
                        string opponentplay = (CurrentPlayer == "blackplayer") ? "white" : "black";

                        digits = fromPos.Select(c => int.Parse(c.ToString())).ToArray();
                        int currentrow = digits[0];
                        int currentcol = digits[1];

                        digits = new int[0];
                        digits = toPos.Select(c => int.Parse(c.ToString())).ToArray();
                        int targetedrow = digits[0];
                        int targetedcol = digits[1];

                        if ((pieceName != "") && (pieceName.Contains(currentplay)))
                        {
                            LastMoveWasCastleChange = false;
                            LastMoveWasPawnPromotion = false;   
                            //#######CHESS GAMEFLOW:########//

                            //step1: First check if the user made move is correct. If incorrect prompt to try again. 
                            if (IsUserMoveCorrect(targetedcol, targetedrow, currentcol, currentrow, pieceName, false, true, false))
                            {
                                // Step2: We know the move is correct. Now, we want to make sure the move doesnt cause the king to be in check.       
                                // Step3: If the move cause the king to be in check the move is invalid.
                                // Exception: For castle change move the "IsThisMoveIsInCheckPosition" function is already done && positions are switched. So, do a quick bool check and move on.

                                bool doesthemovecausescheck = (LastMoveWasCastleChange) ? false : IsThisMoveIsInCheckPosition(currentrow, currentcol, targetedrow, targetedcol, pieceName);
                                if (!doesthemovecausescheck)
                                {
                                    //Once move is made we can assume that king is no longer in check so reassign as false.        
                                    // even if there is check to the king. The below process ### IsKingInCheck(opponentplay) will reassign the check   
                                    whiteKingIsInCheck = false;
                                    blackKingIsInCheck = false;
                                    OkayToCastleBlack = true;
                                    OkayToCastleWhite = true;
                                    if (PlayerPositionsToCheckCurrentPlayerKing != null) { PlayerPositionsToCheckCurrentPlayerKing.Clear(); }

                                    // here check for the promotion procedure. if condition is matched, do the function. If promotion then picture is already updtaed.
                                    // logic behind separating castle change differently then pawn promotion is that,
                                    // Pawn promotion follows the same move as normal pawn hence we can use same check and if its at the end then do pawn promotion funciton.
                                    // whereas the castle change check follows different set of check and move. Hence, the seperation.

                                    if ((pieceName == "black_pawn" && targetedrow == 8) || (pieceName == "white_pawn" && targetedrow == 0))
                                    { PerformPawnPromotion(targetedcol, targetedrow, currentcol, currentrow, pieceName); }

                                    // If the current move was castle then these below function are alredy done. hence jump ahead.            
                                    if (!(LastMoveWasCastleChange || LastMoveWasPawnPromotion))
                                    {
                                        //Step4: The move is correct, it doen't cause the king to be in check. So now, update the chessboard. update the chessmatrix. And Assume the next play is not in check/.
                                        UpdateChessMatrix(currentrow, currentcol, targetedrow, targetedcol, pieceName);

                                        UpdatepictureofChessBoard(pieceName, to, from);         
                                    }

                                    //make sure the king check is done before switching of play.        
                                    //Step5: Now, everything is updated. So, we want to check if the opponent or next user king's position. We want to know if the king is in check.
                                    //Step6: IF the king is in check. We want to then check for checkmate possibilities. Is there any possibilities to block the checkmate or if king can move out of checkmate.       
                                    // Step7: if no move can be made then we declare winner. If there is moves left then, we switch play and continue on.
                                    bool CheckAgainstKing = IsKingInCheck(opponentplay, true);
                                    if (CheckAgainstKing)
                                    {
                                        // if there is check in opponent then show check message.      
                                        if (CurrentPlayer == "whiteplayer") { blackKingIsInCheck = true; OkayToCastleBlack = false; } else { whiteKingIsInCheck = true; OkayToCastleWhite = false; }

                                        //check for checkmate        
                                        if (CheckForCheckMate(opponentplay)) { GameWinner = "Winner is " + CurrentPlayer; }

                                    }
                                    else
                                    {
                                        // here check for stalemate option. If there is no move left for the current player.       
                                        // If the king is in check then game is won but if not in check && no valid moves are available then. Game is drawn.           
                                        if (ThisGameIsStaleMate(opponentplay)) { GameWinner = "Gamedrawn"; }
                                    }

                                    LastMoveWasCastleChange = false;
                                    SwitchPlayer();
                                }
                            }
                            //#### LOOP until the game is ended.
                        }
                    }
                }
                catch (Exception exec) { MessageBox.Show("Error in switching play." + exec.Message); }
            }
        }

        //After move is made update the chessboard images.
        void UpdatepictureofChessBoard(string pieceName, Point to, Point from)
        {
            Panel fromPanel = _chessBoardPanels[from.X, from.Y]; // current position
            Panel toPanel = _chessBoardPanels[to.X, to.Y];       // target position

            PictureBox movingPiece = null;

            // Find the picture box in the source panel
            foreach (Control control in fromPanel.Controls)
            {
                if (control is PictureBox piece)
                {
                    movingPiece = piece;
                    break;
                }
            }

            if (movingPiece != null)
            {
                fromPanel.Controls.Clear(); // remove from current
                toPanel.Controls.Clear();   // remove any piece at destination (capture)

                if (!string.IsNullOrEmpty(pieceName))
                {

                    movingPiece.Image = Image.FromFile($"{ImageLocationPiece}/{pieceName}.png"); // update image
                    toPanel.Controls.Add(movingPiece); // add to new location
                }
            }
        }

        //After move update the chess board. Actual matrix move happens here except the castle change.
        void UpdateChessMatrix(int currentrow, int currentcol, int targetedrow, int targetedcol, string thischesspiece)
        {
            boardpieces[currentrow, currentcol] = "";
            string targetedpeice = boardpieces[targetedrow, targetedcol];
            boardpieces[targetedrow, targetedcol] = thischesspiece;
        }


        //whenever the move is successfull, switch the player
        // After the play is switched and if the current player king is in check.
        // Here possibly implement the check to determine winner.
        // Check for king moves if none then declare winner.
        void SwitchPlayer()
        {
            if (!(GameWinner == "" || GameWinner == null))
            {
                EndOfGame();
                MessageBox.Show("GameOver" + GameWinner);
            }
            else
            {
                if (CurrentPlayer == "whiteplayer")
                {
                    CurrentPlayer = "blackplayer"; txtPlayerTurn.Text = "blackplayer";
                    if (blackKingIsInCheck)
                    {
                        MessageBox.Show("King is in Check.");
                    }
                }
                else
                {
                    CurrentPlayer = "whiteplayer"; txtPlayerTurn.Text = "whiteplayer";
                    if (whiteKingIsInCheck)
                    {
                        MessageBox.Show("King is in Check.");
                    }
                }
            }
        }

        //Function to check for the stalemate.
        //Logic is that as long as there is any move that can be made. Game is on. So, if one more is true exit the check.
        bool ThisGameIsStaleMate(string opponentplay)
        {
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    string thispiece = boardpieces[x, y];

                    if (thispiece != "" && thispiece.Contains(opponentplay))
                    {
                        bool CheckStaleMate = IsUserMoveCorrect(0, 0, y, x, thispiece, true, false, true);

                        if (TempCheckForStaleMate.Count > 0 && CheckStaleMate)
                        {
                            foreach (int[] tempcheckforstalemate in TempCheckForStaleMate) // Iterate through each row
                            {
                                int temprow = tempcheckforstalemate[0];  // Get row
                                int tempcol = tempcheckforstalemate[1];  // Get column

                                if (!(IsThisMoveIsInCheckPosition(x , y ,temprow, tempcol, thispiece)))
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }


        bool CheckForCheckMate(string opponentplay)
        {
            //STEP1: Check if the king has squares to move to avoid check.
            //STEP2: Check if the peices the king is checked can be attacked by any pieces.
            //STEP3: Check if any peices can block the check.
            //NOTE: If any error just set the value false and continue.
            try
            {
                    if (CheckIfKingCanMove(opponentplay))
                    {
                        return  false;
                    }

                    //Idea is that if there is check from two different pieces. The only option is to move the king. And if the king can't move. Thats GAMEE.
                    if (PlayerPositionsToCheckCurrentPlayerKing.Count == 1)
                    {
                        if (CanRemoveCheckGivingPeices(opponentplay))
                        {
                            return  false;
                        }

                        if (CanCheckBeBlocked(opponentplay))
                        {
                            return  false;
                        }
                    }
            }
            catch (Exception excep) { MessageBox.Show(excep.Message); return false; }
            return true;
        }

        bool CheckIfKingCanMove(string opponentplay)
        {
            string kingposition = $"{opponentplay}{"_king"}";
            string playertocheckkingposition = FindCharacterInChessboardMatrix(kingposition);

            //current king row.
            int kingrowvalue = playertocheckkingposition[0] - '0';
            int kingcolvalue = playertocheckkingposition[1] - '0';

            bool CheckStaleMate = IsUserMoveCorrect(0, 0, kingcolvalue, kingrowvalue, kingposition, false, false, true);

            if (TempCheckForStaleMate.Count > 0 && CheckStaleMate)
            {
                foreach (int[] tempcheckforstalemate in TempCheckForStaleMate) // Iterate through each row
                {
                    int temprow = tempcheckforstalemate[0];  // Get row
                    int tempcol = tempcheckforstalemate[1];  // Get column

                    if (!(IsThisMoveIsInCheckPosition(kingrowvalue, kingcolvalue, temprow, tempcol, kingposition)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        bool CanRemoveCheckGivingPeices(string opponentplay)
        {
            // we know that there is only one item in the list
            int[] pieceking = PlayerPositionsToCheckCurrentPlayerKing[0];

            if (pieceking != null)
            {
                //current king row.
                int targetrow = pieceking[0];
                int targetcol = pieceking[1];

                if (CanAnyPlayerMoveToThisChessSquare(opponentplay, targetrow, targetcol))
                {
                    return true;
                }
            }

            return false;
        }

        bool CanCheckBeBlocked(string opponentplay)
        {
            //Now we have check for rook style check && bishop style move.
            //If no move while knight has a check then its game over. Also, same for pawn.

            string kingposition = $"{opponentplay}{"_king"}";
            string playertocheckkingposition = FindCharacterInChessboardMatrix(kingposition);

            //current king row.
            int kingrowvalue = playertocheckkingposition[0] - '0';
            int kingcolvalue = playertocheckkingposition[1] - '0';

            //Check from only one piece.
            // we know that there is only one item in the list
            int[] pieceking = PlayerPositionsToCheckCurrentPlayerKing[0];

            int checkfromrow = pieceking[0];
            int checkfromcol = pieceking[1];

            int rowdiff = Math.Abs(kingrowvalue - checkfromrow);
            int coldiff = Math.Abs(kingcolvalue - checkfromcol);

            if (kingrowvalue == checkfromrow || kingcolvalue == checkfromcol)
            {
                bool CheckStaleMate = GetRookStylePossibleMoves(checkfromrow, checkfromcol, kingcolvalue, kingrowvalue, kingposition, true);

                if (TempCheckForStaleMate.Count > 0 && CheckStaleMate)
                {
                    foreach (int[] tempcheckforstalemate in TempCheckForStaleMate) // Iterate through each row
                    {
                        int temprow = tempcheckforstalemate[0];  // Get row
                        int tempcol = tempcheckforstalemate[1];  // Get column

                        if (CanAnyPlayerMoveToThisChessSquare(opponentplay, temprow, tempcol))
                        {
                            return true;
                        }
                    }
                }
            }
            else if (rowdiff == coldiff)
            {
                bool CheckStaleMate = GetBishopStylePossibleMoves (checkfromrow,checkfromcol,kingcolvalue,kingrowvalue,kingposition,true);

                if (TempCheckForStaleMate.Count > 0 && CheckStaleMate)
                {
                    foreach (int[] tempcheckforstalemate in TempCheckForStaleMate) // Iterate through each row
                    {
                        int temprow = tempcheckforstalemate[0];  // Get row
                        int tempcol = tempcheckforstalemate[1];  // Get column

                        if (CanAnyPlayerMoveToThisChessSquare(opponentplay, temprow, tempcol))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        bool CanAnyPlayerMoveToThisChessSquare(string opponentplay, int targetrow, int targetcol)
        {
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    string thispiece = boardpieces[x, y];

                    if (thispiece != "" && thispiece.Contains(opponentplay) && !thispiece.Contains("king"))
                    {
                        if (IsUserMoveCorrect(targetcol, targetrow, y, x, thispiece, true, false, false)) 
                        { 
                            if (!(IsThisMoveIsInCheckPosition(x, y, targetrow, targetcol, thispiece)))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        //When this move is made does it affects the kings position. 
        bool IsThisMoveIsInCheckPosition(int currentrow, int currentcol, int targetedrow, int targetedcol, string pieceName)
        {
            string currentplay = (pieceName.Contains("black")) ? "black" : "white";

            //Assume the move is made
            string tempchesspiece = "";
            tempchesspiece = boardpieces[targetedrow, targetedcol];
            boardpieces[currentrow, currentcol] = "";
            boardpieces[targetedrow, targetedcol] = pieceName;

            //now check if the king will still be in check.
            if (IsKingInCheck(currentplay, false))
            {
                boardpieces[currentrow, currentcol] = pieceName;
                boardpieces[targetedrow, targetedcol] = tempchesspiece;
                return true;
            }
            else
            {
                // set the value back to normal
                boardpieces[currentrow, currentcol] = pieceName;
                boardpieces[targetedrow, targetedcol] = tempchesspiece;
                return false;
            }
        }

        //checked here
        bool IsKingInCheck(string whatplayertocheck, bool performfullpiecescheck)
        {
            string thischesspiece;
            List<int[]> templist = new List<int[]>();

            try
            {
                string whichpiecetocheck = $"{whatplayertocheck}{"_king"}";
                string playertocheckkingposition = FindCharacterInChessboardMatrix(whichpiecetocheck);

                int kingrowvalue = playertocheckkingposition[0] - '0';
                int kingcolvalue = playertocheckkingposition[1] - '0';

                for (int x = 0; x < 8; x++)
                {
                    for (int y = 0; y < 8; y++)
                    {
                        thischesspiece = boardpieces[x, y];
   
                        if ((!thischesspiece.StartsWith(whatplayertocheck)) && (thischesspiece != ""))
                        {
                            if (IsUserMoveCorrect(kingcolvalue, kingrowvalue, y, x, thischesspiece, true, false, false))
                            {
                                if (performfullpiecescheck){
                                        templist.Add(new int[] { x, y });   
                                }
                                else{ 
                                    return true;
                                }
                            }
                        }
                    }
                }    
            }
            catch (Exception thise) { MessageBox.Show(thise.Message); return true; }

            if (performfullpiecescheck && templist.Count > 0) { PlayerPositionsToCheckCurrentPlayerKing.AddRange(templist); return true; }
            return false;
        }

        //check the move for the chess pieces
        bool IsUserMoveCorrect(int targetedcol, int targetedrow, int currentcol, int currentrow, string thischesspiece, bool checkingforcheck, bool showmessage, bool checkforstalemate)
        {
            bool returnvalue = false;

            string pieceType = thischesspiece.Split('_')[1];
            try
            {
                switch (pieceType)
                {
                    case "pawn":
                        returnvalue = pawn(targetedcol, targetedrow, currentcol, currentrow, thischesspiece, checkingforcheck, showmessage, checkforstalemate);
                        break;
                    case "knight":
                        returnvalue = knight(targetedcol, targetedrow, currentcol, currentrow, thischesspiece, checkingforcheck, showmessage, checkforstalemate);
                        break;
                    case "king":
                        returnvalue = king(targetedcol, targetedrow, currentcol, currentrow, thischesspiece, checkingforcheck, showmessage, checkforstalemate);
                        break;
                    case "rook":
                        returnvalue = rook(targetedcol, targetedrow, currentcol, currentrow, thischesspiece, checkingforcheck, showmessage, checkforstalemate);
                        break;
                    case "bishop":
                        returnvalue = bishop(targetedcol, targetedrow, currentcol, currentrow, thischesspiece, checkingforcheck, showmessage, checkforstalemate);
                        break;
                    case "queen":
                        returnvalue = queen(targetedcol, targetedrow, currentcol, currentrow, thischesspiece, checkingforcheck, showmessage, checkforstalemate);
                        break;
                    default:
                        MessageBox.Show("Unknown piece type.");
                        break;
                }
            }
            catch (Exception thise) { MessageBox.Show(thise.Message); }
            return returnvalue;
        }

        // Get characters from the chessboard
        string FindCharacterInChessboardMatrix(string chesspiece)
        {
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    string thispiece = boardpieces[x, y];

                    if (thispiece == chesspiece)
                    {
                        return $"{x}{y}";
                    }
                }
            }
            return "";
        }


        // Check if the selected row,col is empty and has opponent peices
        bool IsOkayMove(int row, int col, string usermove)
        {
            string thispiece = boardpieces[row, col];

            //if the player selected & Opponent character is different. move is valid.
            if ((thispiece == "") || (!thispiece.Contains(usermove)))
            {
                return true;
            }
            return false;
        }


        ////Chess individual move Logic
        ///Move for all the chess pieces

        bool king(int targetedcol, int targetedrow, int currentcol, int currentrow, string thischesspiece, bool checkingforcheck, bool showmessage, bool checkforstalemate)
        {
            string PlayerSelected;
            bool checkforcastle;
            string ErrorMoveMessage = "Invalid move for King";

            int rowdiff = Math.Abs(currentrow - targetedrow);
            int coldiff = Math.Abs(currentcol - targetedcol);

            if (thischesspiece.Contains("white")) { PlayerSelected = "white"; checkforcastle = OkayToCastleWhite; } else { PlayerSelected = "black"; checkforcastle = OkayToCastleBlack; }

            if (checkforstalemate) { TempCheckForStaleMate.Clear(); } // as we are making a new list. clear the older content.

            try
            {
                if ((checkforcastle) && (boardpieces[targetedrow, targetedcol].Contains("rook")) && (boardpieces[targetedrow, targetedcol].Contains(PlayerSelected)) && (targetedrow == currentrow) && (targetedcol == 0 || targetedcol == 7))
                {
                    // this is castle change check it.
                    if (CheckForCastleChange(targetedcol, targetedrow, currentcol, currentrow, thischesspiece))
                    {
                        return true;
                    }
                    else { ErrorMoveMessage = "Invalid move for castle."; }
                }
                else
                {
                    if (((checkforstalemate) || (rowdiff == 1 || rowdiff == 0) && (coldiff == 1 || coldiff == 0)))
                    {
                        if (GetKingPossibleMoves(currentrow, currentcol, targetedcol, targetedrow, PlayerSelected, checkforstalemate))
                        {
                            return true;
                        }
                    }
                }
            }
            catch { MessageBox.Show(ErrorMoveMessage); return false; }

            if (showmessage) { MessageBox.Show(ErrorMoveMessage); }
            return false;
        }

        bool knight(int targetedcol, int targetedrow, int currentcol, int currentrow, string thischesspiece, bool checkingforcheck, bool showmessage, bool checkforstalemate)
        {
            string ErrorMoveMessage = "Invalid move for Knight";
            string PlayerSelected;

            if (thischesspiece.Contains("white")) { PlayerSelected = "white"; } else { PlayerSelected = "black"; }

            if (checkforstalemate) { TempCheckForStaleMate.Clear(); } // as we are making a new list. clear the older content.

            int rowdiff = Math.Abs(currentrow - targetedrow);
            int coldiff = Math.Abs(currentcol - targetedcol);

            try
            {
                if (((checkforstalemate) || (rowdiff == 2 && coldiff == 1) || (rowdiff == 1 && coldiff == 2)))
                {
                    if (GetKnightPossibleMoves(currentrow, currentcol, targetedcol, targetedrow, PlayerSelected, checkforstalemate))
                    {
                        return true;
                    }
                }
            }
            catch { MessageBox.Show(ErrorMoveMessage); return false; }

            if (showmessage) { MessageBox.Show(ErrorMoveMessage); }
            return false;
        }

        bool pawn(int targetedcol, int targetedrow, int currentcol, int currentrow, string thischesspiece, bool checkingforcheck, bool showmessage, bool checkforstalemate)
        {
            // set default value to return to be false
            string ErrorMoveMessage = "Invalid move for pawn.";
            string PlayerSelected;

            if (thischesspiece.Contains("white")) { PlayerSelected = "white"; } else { PlayerSelected = "black"; }

            if (checkforstalemate) { TempCheckForStaleMate.Clear(); } // as we are making a new list. clear the older content.

            int rowdiff = Math.Abs(currentrow - targetedrow);
            int coldiff = Math.Abs(currentcol - targetedcol);

            try
            {
                if (((checkforstalemate) || (rowdiff == 1 || rowdiff == 2) && (coldiff == 1 || coldiff == 0)))
                {
                    if (GetPawnPossibleMoves(currentrow, currentcol, targetedcol, targetedrow, PlayerSelected, checkforstalemate))
                    {
                        return true;
                    }
                }
            }
            catch { MessageBox.Show(ErrorMoveMessage); return false; }

            //Show error message
            if (showmessage) { MessageBox.Show(ErrorMoveMessage); }
            return false;
        }

        bool rook(int targetedcol, int targetedrow, int currentcol, int currentrow, string thischesspiece, bool checkingforcheck, bool showmessage, bool checkforstalemate)
        {
            string ErrorMoveMessage = "Invalid move for rook.";
            string PlayerSelected;

            if (thischesspiece.Contains("white")) { PlayerSelected = "white"; } else { PlayerSelected = "black"; }

            if (checkforstalemate) { TempCheckForStaleMate.Clear(); } // as we are making a new list. clear the older content.

            try
            {
                if (((checkforstalemate) || (targetedrow == currentrow || targetedcol == currentcol)))
                {
                    if (GetRookStylePossibleMoves(currentrow, currentcol, targetedcol, targetedrow, PlayerSelected, checkforstalemate))
                    {
                        return true;
                    }
                }
            }
            catch { MessageBox.Show(ErrorMoveMessage); return false;}

            if (showmessage) { MessageBox.Show(ErrorMoveMessage); }
            return false;
        }

        bool bishop(int targetedcol, int targetedrow, int currentcol, int currentrow, string thischesspiece, bool checkingforcheck, bool showmessage, bool checkforstalemate)
        {
            string ErrorMoveMessage = "Invalid move for bishop.";
            string PlayerSelected;

            if (thischesspiece.Contains("white")) { PlayerSelected = "white"; } else { PlayerSelected = "black"; }

            if (checkforstalemate) { TempCheckForStaleMate.Clear(); } // as we are making a new list. clear the older content.

            int rowdiff = Math.Abs(currentrow - targetedrow);
            int coldiff = Math.Abs(currentcol - targetedcol);

            try
            {
                if (((checkforstalemate) || (rowdiff == coldiff)))
                {
                    if (GetBishopStylePossibleMoves(currentrow, currentcol, targetedcol, targetedrow, PlayerSelected, checkforstalemate))
                    {
                        return true;
                    }
                }
            }
            catch { MessageBox.Show(ErrorMoveMessage); return false; }

            if (showmessage) { MessageBox.Show(ErrorMoveMessage); }
            return false;
        }

        // queen move is combined move of rook and bishop. So, it will call these move and combine to get the queen moves.
        bool queen(int targetedcol, int targetedrow, int currentcol, int currentrow, string thischesspiece, bool checkingforcheck, bool showmessage, bool checkforstalemate)
        {
            string ErrorMoveMessage = "Invalid move for rook.";
            string PlayerSelected;

            if (thischesspiece.Contains("white")) { PlayerSelected = "white"; } else { PlayerSelected = "black"; }

            if (checkforstalemate) { TempCheckForStaleMate.Clear(); } // as we are making a new list. clear the older content.

            int rowdiff = Math.Abs(currentrow - targetedrow);
            int coldiff = Math.Abs(currentcol - targetedcol);

            try
            {
                // Get rook style move 
                if (((checkforstalemate) || (targetedrow == currentrow || targetedcol == currentcol)))
                {
                    if (GetRookStylePossibleMoves(currentrow, currentcol, targetedcol, targetedrow, PlayerSelected, checkforstalemate))
                    {
                        return true;
                    }
                }

                // get bishop style move
                if (((checkforstalemate) || (rowdiff == coldiff)))
                {
                    if (GetBishopStylePossibleMoves(currentrow, currentcol, targetedcol, targetedrow, PlayerSelected, checkforstalemate))
                    {
                        return true;
                    }
                }
            }
            catch { MessageBox.Show(ErrorMoveMessage); return false; }

            //Show error message
            if (showmessage) { MessageBox.Show(ErrorMoveMessage); }
            return false;
        }

        bool GetPawnPossibleMoves(int currentrow, int currentcol, int targetedcol, int targetedrow, string PlayerSelected, bool checkforstalemate)
        {
            List<int[]> possiblemoveslist = new List<int[]>();
            int temprow = 0;
            int tempcol = 0;
            
            try
            {
                // pawn move is always forward && one step across for attack, (x + 1, y +- 1). On empty boxes it can move one or two step ahead, (x + 1 or x + 2, y) . Also, only first move is two step.
                for (int y = -1; y <= 1; y++)
                {
                    for (int x = 1; x <= 2; x++)
                    {
                        string thischaracter = "";
                        if (y == 0 && (x == 1 || x == 2))
                        {
                            if ((PlayerSelected == "black" && x == 2 && currentrow != 1) || (PlayerSelected == "white" && x == 2 && currentrow != 6))
                            { }//two moves can only be made from first position.}
                            else
                            {
                                if (PlayerSelected == "black")
                                {
                                    temprow = currentrow + x; tempcol = currentcol;
                                }
                                else if (PlayerSelected == "white")
                                {
                                    temprow = currentrow - x; tempcol = currentcol;
                                }

                                if ((temprow >= 0 && temprow <= 7) && (tempcol >= 0 && tempcol <= 7))
                                {
                                    thischaracter = boardpieces[temprow, tempcol];

                                    if (thischaracter != "")
                                    { break; }
                                    else
                                    {
                                        if (checkforstalemate)
                                        {
                                            possiblemoveslist.Add(new int[] { temprow, tempcol });
                                        }
                                        else if (!checkforstalemate && (temprow == targetedrow && tempcol == targetedcol))
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                        else if (Math.Abs(y) == 1 && x == 1)
                        {
                            if (PlayerSelected == "black")
                            {
                                temprow = currentrow + 1; tempcol = currentcol + y;
                            }
                            else if (PlayerSelected == "white")
                            {
                                temprow = currentrow - 1; tempcol = currentcol + y;
                            }

                            if ((temprow >= 0 && temprow <= 7) && (tempcol >= 0 && tempcol <= 7))
                            { 
                                thischaracter = boardpieces[temprow, tempcol];

                                if (!thischaracter.Contains(PlayerSelected) && thischaracter != "")
                                {
                                    if (checkforstalemate)
                                    {
                                        possiblemoveslist.Add(new int[] { temprow, tempcol });
                                    }
                                    else if (!checkforstalemate && (temprow == targetedrow && tempcol == targetedcol))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch { possiblemoveslist.Clear(); }

            if (possiblemoveslist.Count > 0 && checkforstalemate)
            { TempCheckForStaleMate.AddRange(possiblemoveslist); return true; }

            return false;
        }

        bool GetKingPossibleMoves(int currentrow, int currentcol, int targetedcol, int targetedrow, string PlayerSelected, bool checkforstalemate)
        {
            List<int[]> possiblemoveslist = new List<int[]>();
            int temprow = 0;
            int tempcol = 0;

            try
            {
                // king moves is (x +- 1 , y +- 1)
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        temprow = currentrow + x;
                        tempcol = currentcol + y;

                        if ((temprow >= 0 && temprow <= 7) && (tempcol >= 0 && tempcol <= 7))
                        {
                            if (IsOkayMove(temprow, tempcol, PlayerSelected))
                            {
                                if (checkforstalemate)
                                {
                                    possiblemoveslist.Add(new int[] { temprow, tempcol });
                                }
                                else if (!checkforstalemate && (temprow == targetedrow && tempcol == targetedcol))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            catch { possiblemoveslist.Clear(); return false; }

            if (possiblemoveslist.Count > 0 && checkforstalemate)
            { TempCheckForStaleMate.AddRange(possiblemoveslist); return true; }

            return false;
        }

        bool GetKnightPossibleMoves(int currentrow, int currentcol, int targetedcol, int targetedrow, string PlayerSelected, bool checkforstalemate)
        {
            List<int[]> possiblemoveslist = new List<int[]>();
            int temprow = 0;
            int tempcol = 0;

            try
            {
                // the move of the knight will always have move (x +- 2, y+- 1) or (x +- 1 , y +- 2). SO, if has to be within 2*2 square box everyhting else is false. 
                for (int x = -2; x <= 2  ; x++)
                {
                    for (int y = -2; y <= 2 ; y++)
                    {
                        if ((Math.Abs(x) != Math.Abs(y)) && y != 0 && x != 0) {
                            temprow = currentrow + x;
                            tempcol = currentcol + y;

                            if ((temprow >= 0 && temprow <= 7) && (tempcol >= 0 && tempcol <= 7))
                            {
                                if (IsOkayMove(temprow, tempcol, PlayerSelected)) {
                                    if (checkforstalemate)
                                    {
                                        possiblemoveslist.Add(new int[] { temprow, tempcol });
                                    }
                                    else if (!checkforstalemate && (temprow == targetedrow && tempcol == targetedcol))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch { possiblemoveslist.Clear(); }

            if (possiblemoveslist.Count > 0 && checkforstalemate)
            { TempCheckForStaleMate.AddRange(possiblemoveslist); return true; }

            return false;
        }


        //
        bool GetBishopStylePossibleMoves(int currentrow, int currentcol, int targetedcol, int targetedrow, string PlayerSelected, bool checkforstalemate)
        {
            List<int[]> possiblemoveslist = new List<int[]>();
            int rowchangeby = 0;
            int colchangeby = 0;

            try
            {
                // bishop move is diagonal move on all directions. Hence the following logic, (x +- n, y +- n). n here is the value of how many squares it can jump and it has to be same. 
                for (int i = 1; i <= 4; i++)
                {
                    string thisstring = i.ToString();
                    switch (thisstring)
                    {
                        case "1":
                            rowchangeby = 1; colchangeby = 1; break;
                        case "2":
                            rowchangeby = 1; colchangeby = -1; break;
                        case "3":
                            rowchangeby = -1; colchangeby = 1; break;
                        case "4":
                            rowchangeby = -1; colchangeby = -1; break;
                    }

                    int temprow = currentrow + rowchangeby;
                    int tempcol = currentcol + colchangeby;
                    string thischaracter = "";

                    while (((temprow >= 0 && temprow <= 7) && (tempcol >= 0 && tempcol <= 7))) {
                        thischaracter = boardpieces[temprow, tempcol];
                        if (thischaracter == "")
                        {
                            if (checkforstalemate)
                            {
                                possiblemoveslist.Add(new int[] { temprow, tempcol });
                            }
                            else if (!checkforstalemate && (temprow == targetedrow && tempcol == targetedcol))
                            {
                                return true;
                            }
                            // add the value for next loop.
                            temprow = temprow + rowchangeby;
                            tempcol = tempcol + colchangeby;
                        }
                        else
                        {
                            if (IsOkayMove(temprow,tempcol, PlayerSelected))
                            {
                                if (checkforstalemate)
                                {
                                    possiblemoveslist.Add(new int[] { temprow, tempcol });
                                }
                                else if (!checkforstalemate && (temprow == targetedrow && tempcol == targetedcol))
                                {
                                    return true;
                                }
                            }
                            break;
                        }
                    }
                    
                }
            }
            catch { possiblemoveslist.Clear(); }

            if (possiblemoveslist.Count > 0 && checkforstalemate)
            { TempCheckForStaleMate.AddRange(possiblemoveslist); return true; }

            return false;
        }

        bool GetRookStylePossibleMoves(int currentrow, int currentcol, int targetedcol, int targetedrow, string PlayerSelected, bool checkforstalemate)
        {
            List<int[]> possiblemoveslist = new List<int[]>();
            int rowchangeby = 0;
            int colchangeby = 0;

            try
            {
                // in this rook move the 
                for (int i = 1; i <= 4; i++)
                {
                    string thisstring = i.ToString();
                    switch (thisstring)
                    {
                        case "1":
                            rowchangeby = 0; colchangeby = 1; break;
                        case "2":
                            rowchangeby = 0; colchangeby = -1; break;
                        case "3":
                            rowchangeby = 1; colchangeby = 0; break;
                        case "4":
                            rowchangeby = -1; colchangeby = 0; break;
                    }

                    int tempcol = currentcol + colchangeby;
                    int temprow = currentrow + rowchangeby;
                    string thischaracter = "";

                    while (((temprow >= 0 && temprow <= 7) && (tempcol >= 0 && tempcol <= 7)))
                    {
                        thischaracter = boardpieces[temprow, tempcol];

                        if (thischaracter == "")
                        {
                            if (checkforstalemate)
                            {
                                possiblemoveslist.Add(new int[] { temprow, tempcol });
                            }
                            else if (!checkforstalemate && (temprow == targetedrow && tempcol == targetedcol))
                            {
                                return true;
                            }

                            // add the value for next loop.
                            tempcol = tempcol + colchangeby;
                            temprow = temprow + rowchangeby;
                        }
                        else
                        {
                            if (IsOkayMove(temprow, tempcol, PlayerSelected))
                            {
                                if (checkforstalemate)
                                {
                                    possiblemoveslist.Add(new int[] { temprow, tempcol });
                                }
                                else if (!checkforstalemate && (temprow == targetedrow && tempcol == targetedcol))
                                {
                                    return true;
                                }
                            }
                            break;
                        }
                    }
                }
            }
            catch { possiblemoveslist.Clear(); }

            if (possiblemoveslist.Count > 0 && checkforstalemate)
            { TempCheckForStaleMate.AddRange(possiblemoveslist); return true; }

            return false;
        }


        // routine for castle change
        bool CheckForCastleChange(int targetedcol, int targetedrow, int currentcol, int currentrow, string thischesspiece)
        {
            bool valuetoreturn = false;
            string PlayerSelected;
            bool okaytocastle = true;

            int rookrow = 0; int rookcol = 0; int kingrow = 0; int kingcol = 0;
            Point kingfrom = new Point(0, 0); Point kingto = new Point(0, 0); Point rookfrom = new Point(0, 0); Point rookto = new Point(0, 0);
            bool okaytoupdatechessboard = false;

            if (thischesspiece.Contains("white")) { PlayerSelected = "white"; } else { PlayerSelected = "black"; }

            int changecolby = (targetedcol > currentcol) ? +1 : -1;
            int tempcol = currentcol + changecolby;
            string thischaracter = "";

            // the square boxes must be empty between the king && rook for castle change
            while ((tempcol > 0 && tempcol < 7 ))
            {
                thischaracter = boardpieces[currentrow, tempcol];
                if (thischaracter != "")
                {
                    okaytocastle = false;
                    break;
                }
                tempcol = tempcol + changecolby;
            }

            try
            {
                if (okaytocastle)
                {
                    if (PlayerSelected == "white" && targetedcol == 7)
                    {
                        kingrow = 7; kingcol = 6;
                        rookrow = 7; rookcol = 5;

                        kingfrom = new Point(4, 7);
                        kingto = new Point(6, 7);

                        rookfrom = new Point(7, 7);
                        rookto = new Point(5, 7);
                    }
                    else if (PlayerSelected == "white" && targetedcol == 0)
                    {
                        kingrow = 7; kingcol = 2;
                        rookrow = 7; rookcol = 3;

                        kingfrom = new Point(4, 7);
                        kingto = new Point(2, 7);

                        rookfrom = new Point(0, 7);
                        rookto = new Point(3, 7);
                    }
                    else if (PlayerSelected == "black" && targetedcol == 7)
                    {
                        kingrow = 0; kingcol = 6;
                        rookrow = 0; rookcol = 5;

                        kingfrom = new Point(4, 0);
                        kingto = new Point(6, 0);

                        rookfrom = new Point(7, 0);
                        rookto = new Point(5, 0);
                    }
                    else if (PlayerSelected == "black" && targetedcol == 0)
                    {
                        kingrow = 0; kingcol = 2;
                        rookrow = 0; rookcol = 3;

                        kingfrom = new Point(4, 0);
                        kingto = new Point(2, 0);

                        rookfrom = new Point(0, 0);
                        rookto = new Point(3, 0);
                    }

                    // king can't move to check position so, check against it.
                    if (!(IsThisMoveIsInCheckPosition(currentrow, currentcol, kingrow, kingcol, thischesspiece)))
                    {
                        okaytoupdatechessboard = true;
                    }

                    //Update the containts of the chessboard && images as well.
                    if (okaytoupdatechessboard)
                    {
                        // change pictures
                        LastMoveWasCastleChange = true;
                        if (PlayerSelected == "white") { OkayToCastleWhite = false; } else { OkayToCastleBlack = false; }

                        boardpieces[targetedrow, targetedcol] = "";
                        boardpieces[currentrow, currentcol] = "";

                        boardpieces[kingrow, kingcol] = PlayerSelected + "_king";
                        boardpieces[rookrow, rookcol] = PlayerSelected + "_rook";

                        UpdatepictureofChessBoard(PlayerSelected + "_king", kingto, kingfrom);
                        UpdatepictureofChessBoard(PlayerSelected + "_rook", rookto, rookfrom);

                        valuetoreturn = true;
                    }
                }
            }
            catch { valuetoreturn = false; }
            return valuetoreturn;
        }

        void PerformPawnPromotion(int targetedcol, int targetedrow, int currentcol, int currentrow, string thischesspiece)
        {

            string PlayerSelected;
            string PlayerToBePromoted = "";
            string tempchessboardvalue = "";

            if (thischesspiece.Contains("white")) { PlayerSelected = "white"; } else { PlayerSelected = "black"; }

            PlayerToBePromoted = PlayerSelected + "_queen";

            Point from = new Point (currentcol, currentrow); Point to = new Point(targetedcol, targetedrow);

            tempchessboardvalue = boardpieces[targetedrow, targetedcol];
            boardpieces[targetedrow, targetedcol] = PlayerToBePromoted; // promotion will always be queen.
            boardpieces[currentrow, currentcol] = "";

            try
            {
                UpdatepictureofChessBoard(PlayerToBePromoted, to, from);

                LastMoveWasPawnPromotion = true; // After update set the value to be true, 
            }
            catch { LastMoveWasPawnPromotion = false; boardpieces[targetedrow, targetedcol] = tempchessboardvalue; boardpieces[currentrow, currentcol] = thischesspiece; }

        }
    }
}

