using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Drawing.Drawing2D;
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

//NOTES:
// 1) Load chess board >> load chess peices >> assign chess players
// 2) Then use player1 (white) makes first play. >> then check if the play is correct. 
// 2.1) if play is correct player2 turn >> if play is incorrect play stay with player1. 
// 2.2) lock play with player2 until the correct play is made.
// 3) The play switches between player 1 and player 2 until one of them wins.

//Design 
// 1) For now use textbox to input user move.
// 2) User selects the peices to move


namespace WindowsFormsApp1
{
    public partial class ChessBoard : Form
    {
        public ChessBoard()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;

            Array.Copy(_chessboardpieces, initialPieces, _chessboardpieces.Length);

            Array.Copy(BlackChessPieces, BlackChess, BlackChessPieces.Length);

            Array.Copy(WhiteChessPieces, WhiteChess, WhiteChessPieces.Length);
        }

        // class member array of Panels to track chessboard tiles
        private Panel[,] _chessBoardPanels;

        const int tileSize = 70;
        const int gridSize = 8;

        // location where chess pieces are stored 
        readonly string ImageLocationPiece = "C:\\Users\\abhis\\source\\repos\\ChessProject\\WindowsFormsApp1\\chesspieces";

        //Chess layout
        readonly string[,] _chessboardpieces = new string[8,8]
        {           { "black_rook", "black_knight", "black_bishop", "black_queen", "black_king", "black_bishop", "black_knight", "black_rook" },
                    { "black_pawn", "black_pawn", "black_pawn", "black_pawn", "black_pawn", "black_pawn", "black_pawn", "black_pawn" },
                    { "", "", "", "", "", "", "", "" },
                    { "", "", "", "", "", "", "", "" },
                    { "", "", "", "", "", "", "", "" },
                    { "", "", "", "", "", "", "", "" },
                    { "white_pawn", "white_pawn", "white_pawn", "white_pawn", "white_pawn", "white_pawn", "white_pawn", "white_pawn" },
                    { "white_rook", "white_knight", "white_bishop", "white_queen", "white_king", "white_bishop", "white_knight", "white_rook" }
        };

        // standard chess peices foreach players
        readonly string[] BlackChess = new string[] { "black_rook", "black_knight", "black_bishop", "black_queen", "black_king", "black_bishop", "black_knight", "black_rook", "black_pawn", "black_pawn", "black_pawn", "black_pawn", "black_pawn", "black_pawn", "black_pawn", "black_pawn" };
        readonly string[] WhiteChess = new string[] { "white_pawn", "white_pawn", "white_pawn", "white_pawn", "white_pawn", "white_pawn", "white_pawn", "white_pawn", "white_rook", "white_knight", "white_bishop", "white_queen", "white_king", "white_bishop", "white_knight", "white_rook" };

        // will start with white and change every turn white to black and vice-versa.
        string CurrentPlayer;

        public string[,] initialPieces = new string[8,8];
        public string[] BlackChessPieces = new string[16];
        public string[] WhiteChessPieces = new string[16];

        bool OkayToCastleWhite = true;
        bool OkayToCastleBlack = true;

        bool LastMoveWasCastleChange = false;

        bool blackKingIsInCheck = false;

        bool whiteKingIsInCheck = false;

        private Panel selectedFromPanel = null;
        private Panel selectedToPanel = null;

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

            Array.Copy(_chessboardpieces, initialPieces, _chessboardpieces.Length);
            Array.Copy(BlackChessPieces, BlackChess, BlackChessPieces.Length);
            Array.Copy(WhiteChessPieces, WhiteChess, WhiteChessPieces.Length);

            //Load chessboard && images
            LoadChessPeicesAndBoard();

            // first white player makes a move // can have names from user to display instead
            CurrentPlayer = "whiteplayer";
            txtPlayerTurn.Text = "whiteplayer";

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
                            string pieceName = initialPieces[y,x];

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
            catch (Exception e) { MessageBox.Show( "Error creating chess board" +  e.Message); }
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
                string pieceName = initialPieces[from.Y, from.X]; // Remember: [row, col]
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
                    //MessageBox.Show("Choose correct Move.");
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
                        string pieceName = initialPieces[from.Y, from.X]; // Remember: [row, col]
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
                            // do this if the check is false.
                            LastMoveWasCastleChange = false;

                            if (IsUserMoveCorrect(targetedrow, targetedcol, currentrow, currentcol, pieceName,false,true))
                            {
                                 // Here check if the currentplayer was checked by opponent and new made move negates that.
                                 //also, if it negates then assign the check to be false.
                                if (ThisMoveCausesCheckOnKing(currentrow, currentcol, targetedrow, targetedcol, pieceName))
                                {
                                    MessageBox.Show("");
                                }
                                else
                                {
                                
                                    if (!LastMoveWasCastleChange)
                                    {
                                        UpdatepictureofChessBoard(pieceName, to, from);

                                        UpdateChessMatrix(currentrow, currentcol, targetedrow, targetedcol, pieceName);

                                        //Once move is made we can assume that king is no longer in check so reassign as false.
                                        // even if there is check to the king. The below process ### IsKingInCheck(opponentplay) will reassign the check 
                                        whiteKingIsInCheck = false;
                                        blackKingIsInCheck = false;
                                    }

                                    //make sure the king check is done before switching of play.
                                    if (IsKingInCheck(opponentplay))
                                    {
                                        // if there is check in opponent then show check message.
                                        if (CurrentPlayer == "whiteplayer") { blackKingIsInCheck = true; OkayToCastleBlack = false; } else { whiteKingIsInCheck = true; OkayToCastleWhite = false; }
                                    }

                                    LastMoveWasCastleChange = false;

                                    SwitchPlayer();
                                }                               
                            }
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
                        movingPiece.Image = Image.FromFile($"{ImageLocationPiece}/{pieceName}.png"); // update image if given
                        toPanel.Controls.Add(movingPiece); // add to new location
                    }
                }
        }

        //After move update the chess board. Actual matrix move happens here except the castle change.
        void UpdateChessMatrix(int currentrow, int currentcol, int targetedrow, int targetedcol, string thischesspiece)
        {

            initialPieces[currentrow, currentcol] = "";
            string targetedpeice = initialPieces[targetedrow, targetedcol];            
            initialPieces[targetedrow, targetedcol] = thischesspiece;

            if (targetedpeice != null || targetedpeice != "")
            {
                if (thischesspiece.Contains("black")) { RemoveCharacters(WhiteChessPieces, targetedpeice); } else { RemoveCharacters(BlackChessPieces, targetedpeice); }
            }
        }

        //remove any characters from the individual component container.
        void RemoveCharacters(string[] player, string targetedpeice)
        {
            for (int x = 0; x < 16; x++)
            {
                if (player[x] == targetedpeice)
                {
                    player[x] = "";
                    break; // only remove one character
                }
            }
        }


        //whenever the move is successfull, switch the player
        // After the play is switched and if the current player king is in check.
        // Here possibly implement the check to determine winner.
        // Check for king moves if none then declare winner.
        void SwitchPlayer ()
        {
            if (CurrentPlayer == "whiteplayer"){
                CurrentPlayer = "blackplayer"; txtPlayerTurn.Text = "blackplayer";
                if (blackKingIsInCheck) { 
                    // CheckForCheckMate
                    MessageBox.Show("King is in Check."); 
                }
            }
            else {
                CurrentPlayer = "whiteplayer"; txtPlayerTurn.Text = "whiteplayer";
                if (whiteKingIsInCheck) {
                    // CheckForCheckMate;
                    MessageBox.Show("King is in Check."); 
                }
            }

        }

        // If there is check on the king and the move is made. Check if that move negets the check on the king.
        // If it doesn't the move is invalid. User need to make a correct move.

        bool ThisMoveCausesCheckOnKing(int currentrow, int currentcol, int targetedrow, int targetedcol, string pieceName)
        {
            bool returnvalue = false;
            string currentplay = (CurrentPlayer == "blackplayer") ? "black" : "white";

            //Assume the move is made
            string tempchesspiece = initialPieces[targetedrow, targetedcol];
            initialPieces[currentrow, currentcol] = "";
            initialPieces[targetedrow,targetedcol] = pieceName;

            //now check if the king will still be in check.
            if (IsKingInCheck(currentplay))
            {
                returnvalue = true;
            }

            initialPieces[currentrow, currentcol] = pieceName;
            initialPieces[targetedrow, targetedcol] = tempchesspiece;

            return returnvalue;
        }

        //When this move is made does it affects the kings position. 
        bool IsThisMoveIsInCheckPosition(int currentrow, int currentcol, int targetedrow, int targetedcol, string pieceName)
        {
            bool returnvalue = false;
            string currentplay = (CurrentPlayer == "blackplayer") ? "black" : "white";

            //Assume the move is made
            string tempchesspiece = initialPieces[targetedrow, targetedcol];
            initialPieces[currentrow, currentcol] = "";
            initialPieces[targetedrow, targetedcol] = pieceName;

            //now check if the king will still be in check.
            if(IsKingInCheck(currentplay))
            {  
                returnvalue = true; 
            }

            // set the value back to normal
            initialPieces[currentrow, currentcol] = pieceName;
            initialPieces[targetedrow, targetedcol] = tempchesspiece;

            return returnvalue;
        }

        //Find if the position makes king in check
        bool IsKingInCheck(string whatplayertocheck)
        {
            bool returnvalue = false;
            string whichpiecetocheck = $"{whatplayertocheck }{"_king"}";
            string playertocheckkingposition = FindCharacterInChessboardMatrix(whichpiecetocheck);

            if (playertocheckkingposition == "" || playertocheckkingposition == null)
            {
                //Here maybe put end game option.
                MessageBox.Show("");
            }
            else
            {
                int kingrowvalue = playertocheckkingposition[0] - '0';
                int kingcolvalue = playertocheckkingposition[1] - '0';

                for (int x = 0; x < 8; x++)
                {
                    for (int y = 0; y < 8; y++)
                    {
                        string thischesspiece = initialPieces[x, y]; 

                        if ((!thischesspiece.StartsWith(whatplayertocheck)) && (thischesspiece != ""))
                        {
                            if (IsUserMoveCorrect(kingrowvalue, kingcolvalue, x, y, thischesspiece, true, false))
                            {
                                returnvalue = true;
                                // as this is the check done after the move is made. Change the check bool.
                                break;
                            }
                        }
                    }
                }
            }
            return returnvalue;
        }

        //check the move for the chess pieces
        bool IsUserMoveCorrect(int targetedrow, int targetedcol, int currentrow, int currentcol , string thischesspiece, bool checkingforcheck, bool showmessage)
        {
            bool returnvalue = true;

            string pieceType = thischesspiece.Split('_')[1];

            switch (pieceType)
            {
                case "pawn":
                    returnvalue = pawn(targetedcol, targetedrow, currentcol, currentrow, thischesspiece, checkingforcheck, showmessage);
                    break;
                case "knight":
                    returnvalue = knight(targetedcol, targetedrow, currentcol, currentrow, thischesspiece, checkingforcheck, showmessage);
                    break;
                case "king":
                    returnvalue = king(targetedcol, targetedrow, currentcol, currentrow, thischesspiece, checkingforcheck, showmessage);
                    break;
                case "rook":
                    returnvalue = rook(targetedcol, targetedrow, currentcol, currentrow, thischesspiece, checkingforcheck, showmessage);
                    break;
                case "bishop":
                    returnvalue = bishop(targetedcol, targetedrow, currentcol, currentrow, thischesspiece, checkingforcheck, showmessage);
                    break;
                case "queen":
                    returnvalue = queen(targetedcol, targetedrow, currentcol, currentrow, thischesspiece, checkingforcheck, showmessage);
                    break;
                default:
                    MessageBox.Show("Unknown piece type.");
                    break;
            }
            return returnvalue;
        }


        string FindCharacterInChessboardMatrix(string chesspiece)
        {
            string returnvalue = "";

            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    string thispiece = initialPieces[x, y];

                    if (thispiece == chesspiece)
                    {
                        returnvalue = $"{x}{y}";
                    }
                }
            }
            return returnvalue;
        }


        bool IsChesslocationEmptyUsingMatrix(int row, int col)
        {
            bool returnvalue = false;
                if (initialPieces[row, col] == "")
                {
                    returnvalue = true;
                }
            return returnvalue;
        }


        // Check if the selected row,col is empty and has opponent peices
        bool IsOkayMove(int row, int col, string usermove)
        {
            bool returnvalue = false;

            //if the player selected & Opponent character is different. move is valid.
            if ((initialPieces[row, col] == "") || (!initialPieces[row, col].Contains(usermove)))
            {
                returnvalue = true;
            }
            return returnvalue;
        }


        ////Chess Logic
        ///Move for all the chess pieces
        // thisrows means the new targetedrow && thisoldrow means the currentrow
        // thiscols means the new targetedcol && thisoldcols means the currentcol
        // ALSO, swap the (x,y)coprdinates to (y,x). for matrix check [x,y] and point[y,x] check VERY VERY IMPORTANT.

        bool king(int thiscols, int thisrows, int thisoldcols, int thisoldrows, string thischesspiece, bool checkingforcheck, bool showmessage)
        {
            string PlayerSelected;
            bool checkforcastle;
            bool valuetoreturn = false;
            string ErrorMoveMessage = "Invalid move for King";

            if (thischesspiece.Contains("white"))
            { PlayerSelected = "white"; checkforcastle = OkayToCastleWhite; }
            else { PlayerSelected = "black"; checkforcastle = OkayToCastleBlack; }

            if (!checkingforcheck)
            {
                if ((checkforcastle) && (initialPieces[thisrows, thiscols].Contains("rook")) && (initialPieces[thisrows, thiscols].Contains(PlayerSelected)) && (thisrows == thisoldrows) && (thiscols == 0 || thiscols == 7))
                { // this is castle change check it.
                    if (CheckForCastleChange(thiscols, thisrows, thisoldcols, thisoldrows, thischesspiece))
                    {
                        valuetoreturn = true;
                        ErrorMoveMessage = "";
                    }
                    else { ErrorMoveMessage = "Invalid move for castle."; }
                }
                else if (IsOkayMove(thisrows, thiscols, PlayerSelected))
                {
                    if (((thisrows - thisoldrows) <= 1 && (thisrows - thisoldrows) >= -1) && ((thiscols - thisoldcols) <= 1 && (thiscols - thisoldcols) >= -1))
                    {
                        if (!IsThisMoveIsInCheckPosition(thisoldrows, thisoldcols, thisrows, thiscols, thischesspiece))
                        {
                            valuetoreturn = true;
                            ErrorMoveMessage = "";

                            // when the king is moved the castlechange is not allowed.
                            if (checkforcastle && thischesspiece.Contains("white"))
                            { OkayToCastleWhite = false; }
                            else if (checkforcastle && thischesspiece.Contains("black"))
                            { OkayToCastleBlack = false; }
                        }
                    }
                }
            }

            if (!valuetoreturn && showmessage)  { MessageBox.Show(ErrorMoveMessage); }
            return valuetoreturn;
        }

        bool knight(int thiscols, int thisrows, int thisoldcols, int thisoldrows, string thischesspiece, bool checkingforcheck, bool showmessage)
        {
            // set default value to return to be false
            bool valuetoreturn = false;
            string ErrorMoveMessage = "Invalid move for Knight";
            string PlayerSelected;

            if (thischesspiece.Contains("white"))
            { PlayerSelected = "white"; }
            else { PlayerSelected = "black"; }

            int rowdiff = thisrows - thisoldrows;
            int coldiff = thiscols - thisoldcols;

            // the move of the knight will always have move (x +- 2, y+- 1) or (x +- 1 , y +- 2). SO, if has to be within 2*2 square box everyhting else is false. 
            if ((rowdiff <= 2 && rowdiff >= -2 && coldiff <= 1 && coldiff >= -1) || (rowdiff <= 1 && rowdiff >= -1 && coldiff <= 2 && coldiff >= -2))
            {
                if (rowdiff == -2 || rowdiff == 2)
                {
                    if (coldiff == -1 || coldiff == 1)
                    {
                        // Now check if the new selected move is occupied or not. Can't move into same colour character space,
                        
                        if (IsOkayMove(thisrows, thiscols, PlayerSelected))
                        { valuetoreturn = true; ErrorMoveMessage = ""; }
                    }
                }
                else
                {
                    if (coldiff == -2 || coldiff == 2)
                    {
                        // Now check if the new selected move is occupied or not. Can't move into same colour character space,
                        if (IsOkayMove(thisrows, thiscols, PlayerSelected))
                        {
                            valuetoreturn = true; ErrorMoveMessage = "";
                        }
                    }
                }
            }

            if (!valuetoreturn && showmessage) { MessageBox.Show(ErrorMoveMessage); }
            return valuetoreturn;
        }

        // These will check the move for the pawn
        bool pawn(int thiscols, int thisrows, int thisoldcols, int thisoldrows,  string thischesspiece, bool checkingforcheck, bool showmessage)
        {
            // set default value to return to be false
            bool valuetoreturn = false;
            string ErrorMoveMessage = "Invalid move for pawn.";
            bool CorrectDirectionalMove = false;
            string PlayerSelected;

            if (thischesspiece.Contains("white"))
            { PlayerSelected = "white"; }
            else { PlayerSelected = "black"; }


            // Firstly, check for direction of movement of pawn. pawn move is unidirectional means can only move in one direction.
            if (((PlayerSelected == "black") && (thisoldrows < thisrows)) || ((PlayerSelected == "white") && (thisoldrows > thisrows)))
            {
                // Black pawn chess characters will start at (1) position in our matrix chess board.
                // White pawn chess characters will strart at (6) position in our matrix chess board
                CorrectDirectionalMove = true; //moving forward assign true
            }

            // Only Move ahead if direction is correct.
            if ((CorrectDirectionalMove) && ((thisrows - thisoldrows) <= 2 && (thisrows - thisoldrows) >= -2 && (thiscols - thisoldcols) <= 1 && (thiscols - thisoldcols) >= -1))
            {  
                    //Pawn can only move one position in either direction 
                    // First Make Sure the location infront of pawn is empty.
                    // Get current location && check one step ahead.

                    //Check for move where it attacks opponenet. 
                    if (((thiscols - thisoldcols) == 1 || (thiscols - thisoldcols) == -1) && ((thisrows - thisoldrows) == 1 || (thisrows - thisoldrows) == -1))
                    {
                             // Can only make this move if there is opponent character. 
                            // There is character now check if it is opponent character.
                            string thischaracter = initialPieces[thisrows, thiscols];

                            //if the player selected & Opponent character is different. move is valid.
                            if (!thischaracter.Contains(PlayerSelected) && thischaracter != "") { valuetoreturn = true; ErrorMoveMessage = ""; }

                    }
                    else if (thiscols == thisoldcols)
                    {
                        // here the move is either one step ahead or two step ahead,
                        // Need to make sure the location ahead is empty.
                        bool isthislocationempty;

                            isthislocationempty = PlayerSelected == "black"
                            ? IsChesslocationEmptyUsingMatrix(thisoldrows + 1, thisoldcols)
                            : IsChesslocationEmptyUsingMatrix(thisoldrows - 1, thisoldcols);

                        if (isthislocationempty && !checkingforcheck)
                        {
                            if (((thisrows - thisoldrows) == 1 || (thisrows - thisoldrows) == -1))
                            {
                                // as move is one step ahead its okey to make the move
                                valuetoreturn = true;
                                ErrorMoveMessage = "";
                            }
                            else
                            {
                                // move is two step ahed check for that
                                //Now if two step can only be made from black(1,0) and white(6,0)
                                if (IsChesslocationEmptyUsingMatrix(thisrows, thiscols) && ((PlayerSelected == "black" && thisoldrows ==1) ||(PlayerSelected == "white" && thisoldrows == 6)))
                                {
                                    valuetoreturn = true;
                                    ErrorMoveMessage = "";
                                }
                            }
                        }
                    }
            }

            //Show error message
            if (!valuetoreturn && showmessage) { MessageBox.Show(ErrorMoveMessage); }
            
            return valuetoreturn;
        }

        bool rook(int thiscols, int thisrows, int thisoldcols, int thisoldrows, string thischesspiece, bool checkingforcheck, bool showmessage)
        {
            bool valuetoreturn = false;
            string ErrorMoveMessage = "Invalid move for rook.";
            string PlayerSelected;

            if (thischesspiece.Contains("white"))
            { PlayerSelected = "white"; }
            else { PlayerSelected = "black"; }

            // rook can only travel acroos the column or rows. If not within that range than wrong move
            if (thisrows == thisoldrows || thiscols == thisoldcols)
            {
                if (thisrows == thisoldrows)
                { //move is along the row
                    if (CheckForCharacterBetweenColWithConstRow(thisrows, thisoldcols, thiscols) && IsOkayMove(thisrows, thiscols, PlayerSelected))
                    {
                        // if the rows are empty & the selected row is empty make the move else invalid move.
                        valuetoreturn = true;
                        ErrorMoveMessage = "";
                    }
                }
                else
                {  //move is along the col
                    if (CheckForCharacterBetweenRowWithConstCol(thisoldrows, thisrows, thiscols) && IsOkayMove(thisrows, thiscols, PlayerSelected))
                    {
                        // if the cols are empty & the selected col is empty make the move else invalid move.
                        valuetoreturn = true;
                        ErrorMoveMessage = "";
                    }
                }
            }

            //Show error message
            if (!valuetoreturn && showmessage) { MessageBox.Show(ErrorMoveMessage); }
            return valuetoreturn;
        }

        bool bishop(int thiscols, int thisrows, int thisoldcols, int thisoldrows, string thischesspiece, bool checkingforcheck, bool showmessage)
        {
            bool valuetoreturn = false;
            string ErrorMoveMessage = "Invalid move for bishop.";
            string PlayerSelected;

            if (thischesspiece.Contains("white"))
            { PlayerSelected = "white"; }
            else { PlayerSelected = "black"; }

            int rowdiff = (thisrows < thisoldrows) ? rowdiff = thisoldrows - thisrows : rowdiff = thisrows - thisoldrows;
            int coldiff = (thiscols < thisoldcols) ? coldiff = thisoldcols - thiscols : coldiff = thiscols - thisoldcols;

            // bishop will always travel with (x +- n , y +- n). Hence, the difference in rows & col will always be same. 
            if (rowdiff == coldiff)
            {
                if (CheckForBishopStyleMovement(thisoldrows, thisrows, thisoldcols, thiscols) && IsOkayMove(thisrows, thiscols, PlayerSelected))
                {
                    valuetoreturn = true;
                }
            }

            //Show error message
            if (!valuetoreturn && showmessage) { MessageBox.Show(ErrorMoveMessage); }
            return valuetoreturn;
        }

        bool queen(int thiscols, int thisrows, int thisoldcols, int thisoldrows, string thischesspiece, bool checkingforcheck, bool showmessage)
        {
            bool valuetoreturn = false;
            string ErrorMoveMessage = "Invalid move for queen.";
            string PlayerSelected;

            if (thischesspiece.Contains("white"))
            { PlayerSelected = "white"; }
            else { PlayerSelected = "black"; }

            int rowdiff = (thisrows < thisoldrows) ? rowdiff = thisoldrows - thisrows : rowdiff = thisrows - thisoldrows;
            int coldiff = (thiscols < thisoldcols) ? coldiff = thisoldcols - thiscols : coldiff = thiscols - thisoldcols;

            // First we will determine what kind of move it is//
            //bool MoveLikeKing; bool MoveLikeRook; bool MoveLikeBishop;

            if (((thisrows - thisoldrows) <= 1 && (thisrows - thisoldrows) >= -1) && ((thiscols - thisoldcols) <= 1 && (thiscols - thisoldcols) >= -1))
            { 
              // This move is like king, so as long as the selected location is empty or is of oponent character// can make a move
                if (IsOkayMove(thisrows, thiscols, PlayerSelected))
                {
                    valuetoreturn = true;
                    ErrorMoveMessage = "";
                }
            }
            else if (rowdiff == coldiff)
            {
                // Move like a bishop. So, check for the bishop move & check for individual location
                if (CheckForBishopStyleMovement(thisoldrows, thisrows, thisoldcols, thiscols) && IsOkayMove(thisrows, thiscols, PlayerSelected))
                {
                    valuetoreturn = true;
                    ErrorMoveMessage = "";
                }
            }
            else if (thisrows == thisoldrows || thiscols == thisoldcols)
            {
                // MOve is like a rook. Check against it and output
                if (thisrows == thisoldrows)
                { //move is along the row
                    if (CheckForCharacterBetweenColWithConstRow(thisrows, thisoldcols, thiscols) && IsOkayMove(thisrows, thiscols, PlayerSelected))
                    {
                        // if the rows are empty & the selected row is empty make the move else invalid move.
                        valuetoreturn = true;
                        ErrorMoveMessage = "";
                    }
                }
                else
                {  //move is along the col
                    if (CheckForCharacterBetweenRowWithConstCol(thisoldrows, thisrows, thiscols) && IsOkayMove(thisrows, thiscols, PlayerSelected))
                    {
                        // if the cols are empty & the selected col is empty make the move else invalid move.
                        valuetoreturn = true;
                        ErrorMoveMessage = "";
                    }
                }
            }

            //Show error message
            if (!valuetoreturn && showmessage) { MessageBox.Show(ErrorMoveMessage); }
            return valuetoreturn;
        }


        // check if there is any character between selected moves, keep the row constant & evaluate for changing column.
        // Used for rook & Queen moves && also for castle change
        bool CheckForCharacterBetweenColWithConstRow(int rowinfo, int colinfo, int targetcolinfo){
            bool valuetoreturn = true; // set the default to true if any box is filled then assign false and get out of the loop

            if (colinfo > targetcolinfo)
            {
                for (int i = colinfo - 1; i > targetcolinfo; i--)
                {
                    if (!IsChesslocationEmptyUsingMatrix(rowinfo, i))
                    {
                        valuetoreturn = false;
                        break;
                    }
                }
            }
            else
            {
                for (int i = colinfo + 1; i < targetcolinfo; i++)
                {
                    if (!IsChesslocationEmptyUsingMatrix(rowinfo,i))
                    {
                        valuetoreturn = false;
                        break;
                    }
                }
            }
            return valuetoreturn;
        }

        // Used for rook & Queen moves 
        // check if there is any character between selected moves, keep the col constant & evaluate for changing column.
        bool CheckForCharacterBetweenRowWithConstCol(int rowinfo, int targetrowinfo, int colinfo)
        {
            bool valuetoreturn = true; // set the default to true if any box is filled then assign false and get out of the loop

            if (rowinfo > targetrowinfo)
            {
                for (int i = rowinfo - 1; i > targetrowinfo; i--)
                {
                    if (!IsChesslocationEmptyUsingMatrix(i, colinfo))
                    {
                        valuetoreturn = false;
                        break;
                    }
                }
            }
            else
            {
                for (int i = rowinfo + 1; i < targetrowinfo; i++)
                {
                    if (!IsChesslocationEmptyUsingMatrix(i, colinfo))
                    {
                        valuetoreturn = false;
                        break;
                    }
                }
            }
            return valuetoreturn;
        }

        // check for Bishop style movement// seperated this as this will be useful for Queen movement as well. 
        bool CheckForBishopStyleMovement(int rowinfo, int targetrowinfo, int colinfo, int targetcolinfo)
        {
            bool valuetoreturn = true; // set the default to true if any box is filled then assign false and get out of the loop
            int newrow;
            int newcol;

            int j = (rowinfo < targetrowinfo) ? j = targetrowinfo - rowinfo : j = rowinfo - targetrowinfo;
            for (int i = 1; i < j; i++)
            {
                newrow = (rowinfo < targetrowinfo) ? newrow = rowinfo + i : newrow = rowinfo - i;
                newcol = (colinfo < targetcolinfo) ? newcol = colinfo + i : newcol = colinfo - i;
                if (!IsChesslocationEmptyUsingMatrix(newrow, newcol))
                {
                    valuetoreturn = false;
                    break;
                }
            }
            return valuetoreturn;
        }

        bool CheckForCastleChange(int thiscols, int thisrows, int thisoldcols, int thisoldrows, string thischesspiece)
        {
            bool valuetoreturn = false;
            string PlayerSelected;

            int rookrow = 0; int rookcol = 0; int kingrow = 0; int kingcol = 0; 
            Point kingfrom = new Point (0,0); Point kingto = new Point(0, 0); Point rookfrom = new Point(0, 0); Point rookto = new Point(0, 0);
            bool dataupdated = false;

            if (thischesspiece.Contains("white"))
            { PlayerSelected = "white"; }
            else { PlayerSelected = "black"; }

            int newcols = (thiscols > thisoldcols) ? thiscols - 1 : thiscols + 1;

            if (CheckForCharacterBetweenColWithConstRow(thisrows, thisoldcols, newcols))
            {
                // king can't move to check position so, check against it.
                // Okay to move ahead.
                //Update the containts of the chessboard && images as well.
                if (PlayerSelected == "white" && thiscols == 7 )
                {
                    kingrow = 7; kingcol = 6;
                    rookrow = 7; rookcol = 5;

                    kingfrom = new Point(4, 7);
                    kingto = new Point(6, 7);

                    rookfrom = new Point(7, 7);
                    rookto = new Point(5, 7);

                    if(!IsThisMoveIsInCheckPosition(thisoldrows, thisoldcols, kingrow, kingcol, thischesspiece))
                    {
                        dataupdated = true;
                    }

                }
                else if(PlayerSelected == "white" && thiscols == 0)
                {
                    kingrow = 7; kingcol = 2;
                    rookrow = 7; rookcol = 3;

                    kingfrom = new Point(4, 7);
                    kingto = new Point(2, 7);

                    rookfrom = new Point(0, 7);
                    rookto = new Point(3, 7);

                    if (!IsThisMoveIsInCheckPosition(thisoldrows, thisoldcols, kingrow, kingcol, thischesspiece))
                    {
                        dataupdated = true;
                    }
                }
                else if (PlayerSelected == "black" && thiscols == 7)
                {
                    kingrow = 0; kingcol = 6;
                    rookrow = 0; rookcol = 5;

                    kingfrom = new Point(4, 0);
                    kingto = new Point(6, 0);

                    rookfrom = new Point(7, 0);
                    rookto = new Point(5, 0);

                    if (!IsThisMoveIsInCheckPosition(thisoldrows, thisoldcols, kingrow, kingcol, thischesspiece))
                    {
                        dataupdated = true;
                    }
                }
                else if (PlayerSelected == "black" && thiscols == 0)
                {
                    kingrow = 0; kingcol = 2;
                    rookrow = 0; rookcol = 3;

                    kingfrom = new Point(4, 0);
                    kingto = new Point(2, 0);

                    rookfrom = new Point(0, 0);
                    rookto = new Point(3, 0);

                    if (!IsThisMoveIsInCheckPosition(thisoldrows, thisoldcols, kingrow, kingcol, thischesspiece))
                    {
                        dataupdated = true;
                    }
                }

                if (dataupdated)
                {
                    // change pictures
                    LastMoveWasCastleChange = true;
                    if (PlayerSelected == "white") { OkayToCastleWhite = false; } else { OkayToCastleBlack = false; }

                    initialPieces[thisrows, thiscols] = "";
                    initialPieces[thisoldrows, thisoldcols] = "";

                    initialPieces[kingrow, kingcol] = PlayerSelected + "_king";
                    initialPieces[rookrow, rookcol] = PlayerSelected + "_rook";

                    UpdatepictureofChessBoard(PlayerSelected + "_king", kingto, kingfrom);
                    UpdatepictureofChessBoard(PlayerSelected + "_rook", rookto, rookfrom);

                    valuetoreturn = true;
                }
            }
            return valuetoreturn;
        }
    }
}
