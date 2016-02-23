using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WwaServer
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;

    namespace Natverk_Uppg15a
    {
        // Battlefield Application Protocol (BAP/1.1):

        // Data format: Text.
        // Character encoding: UTF8.
        // Maximal message size: 100 bytes.

        // Request message: "BAP/1.1 START Name R1 C1 R2 C2 ...", 
        // where "Name" is your own name (single word), and "Ri Ci" 
        // are the coordinates (row, column) where you have a ship, 
        // for example "BAP/1.1 START Bill 0 0 0 1 0 2 1 0 1 1 2 0 2 1".

        // Request message: "BAP/1.1 SHOT Row Column Opponent", 
        // where "Row" is the row number and "Column" is the column number 
        // and Opponent is the name (single word) of your opponent, 
        // for example "BAP/1.1 SHOT 1 2 Joe".

        // Response message: "BAP/1.1 OPPONENT Name", 
        // where "Name" (single word) is the name the opponent given to you, 
        // for example "BAP/1.1 OPPONENT Joe".

        // Response message: "BAP/1.1 HIT Row Column IsHit Opponent", 
        // where "IsHit" is either "TRUE" or "FALSE", 
        // and "Row" is the row number and "Column" is the column number,  
        // and Opponent is the name (single word) of your opponent, 
        // for example "BAP/1.1 HIT 1 2 FALSE Joe".

        // Response error message: "BAP/1.1 ERROR Message", 
        // where "Message" is a text string (several words), 
        // for example "BAP/1.1 ERROR Incorrect request.".

        //enum Square
        //{
        //    EMPTY, SHIP
        //}

        class BattlefieldServer
        {

        }
    }

}
