# DesktopChessGame
Desktop Chess Game

Step1: Assign the chess pieces and create a chessboard.
       The chessboard and peices are always same. The top part is black and bottom part is white.
       White Player always goes first and after each valid move the play is switched until the game result.(WIN or Stalemate)
       After GameResult there is option to restart game.

Step2: The user clicks on the desired chesspiece on the chessboard. The chessboard changes the color to green denoting the selected piece.
       Then next click is made on the square where the user wants to move the chesspiece.
       check1: First check is to make sure the first click is made on the chesssquare which is not empty. 
       check2: Then it is made sure that the chesspiece belong to the player whose turn it is.
       check3: Then the chesssquare to where it is moving is checked to be either empty or of opponent's piece.

       Only after these check are valid. The move proceds ahead.
       If any of these checks are invalid, the play stays with the current player and error message is shown.  
       End of this check the chessboard square color changes back to normal color.

Step3: We know the chesspiece to be moved, where the chesspiece is and where its going to.
       check1: Make sure the chesspiece move is valid. For e.g: If king is selected than make sure the move is one step on any direction and the chess square its going is 
       either empty or is opponent.
       check2: Then we check if this move causes check on its king. 

       IF both checks are okay. The actual move is made and chessboard and chesspieces are updated. 
       If any of these checks are invalid, the play stays with the current player and error message is shown.
       
       check3: Then we check if the current move cause check on opponent king.
       check4: Then we check for two things, if opponent king is in check then we check for checkmate or if not we check for stalemate (where there is valid moves for 
       opponent.)
         

Step4: If no game result, the play is switched and game continues.
       If there is game result, either winner or draw. We display the result. There will be option to restart the game.
