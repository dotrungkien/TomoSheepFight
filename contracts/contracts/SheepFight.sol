pragma solidity 0.5.0;

contract SheepFight {
    struct Game {
        string id;
        address leftPlayer;
        address rightPlayer;
    }

    Game[] public games;

    mapping (address => bool) public isPlaying;
    mapping (address => uint) public playerToGame;

    uint public betValue = 1 ether;

    constructor () public {
        games.push(Game("123456", address(0), address(0)));
    }

    function searchGame(string memory gameID)
        public
        returns (uint)
    {
        for (uint i = 0; i < games.length; i++)
        {
            if (compareStringsbyBytes(games[i].id, gameID)) return i;
        }
        return 0;
    }

    function play(string calldata gameID)
        external
        payable
    {
        require(msg.value >= betValue, "must bet 1 value");
        require(!isPlaying[msg.sender], "player must not be in game");


        isPlaying[msg.sender] = true;
        uint gameIdx = searchGame(gameID);
        if (gameIdx == 0) {
            CreateGame(gameID);
        } else {
            JoinGame(gameIdx);
        }
        if (msg.value > betValue) msg.sender.transfer(msg.value - betValue);
    }

    function CreateGame(string memory gameID)
        internal
        returns (uint)
    {
        Game memory newGame = Game(gameID, msg.sender, address(0));
        uint latestGame = games.push(newGame);
        playerToGame[msg.sender] = latestGame;
        return latestGame;
    }

    function JoinGame(uint gameIdx)
        internal
    {
        Game storage game = games[gameIdx];
        game.rightPlayer = msg.sender;
        playerToGame[msg.sender] = gameIdx;
    }


    function endGame(string calldata gameID, bool isWon)
        external
    {
        require(isPlaying[msg.sender], "player must be in game");
        if (isWon) reward(msg.sender);
        isPlaying[msg.sender] = false;
        playerToGame[msg.sender] = 0;
    }

    function forceEndGame()
        external
    {
        isPlaying[msg.sender] = false;
        playerToGame[msg.sender] = -1;
    }

    function compareStringsbyBytes(string memory s1, string memory  s2)
        public
        pure
        returns(bool)
    {
        return keccak256(abi.encodePacked(s1)) == keccak256(abi.encodePacked(s2));
    }

    function reward(address payable to)
        public
        payable
    {
        require(address(this).balance >= 2*betValue, "insufficient balance");
        to.transfer(2*betValue);
    }

    function () external payable {}
}