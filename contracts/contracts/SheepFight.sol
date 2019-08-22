pragma solidity 0.5.0;

contract SheepFight {
    struct Battle {
        string id;
        address leftPlayer;
        address rightPlayer;
        uint result; // 0 - playing, 1 - leftwin, 2 - rightwin
    }

    mapping (address => bool) public isPlaying;
    mapping (address => string) public playerToGame;

    uint public betValue = 1 ether;
    Battle[] public battles;

    event StartBattle(address indexed leftPlayer, address indexed rightPlayer);

    modifier onlyReady() {
        require(!isPlaying[msg.sender], "player must not be in game");
        require(compareStringsbyBytes(playerToGame[msg.sender], ""), "start wrong game ID");
        _;
    }

    modifier onlyPlaying(string memory gameID) {
        require(isPlaying[msg.sender], "player must be in game");
        require(compareStringsbyBytes(playerToGame[msg.sender], gameID), "end wrong game ID");
        _;
    }

    function play(string calldata gameID)
        external
        payable
        onlyReady
    {
        require(msg.value >= betValue, "must bet 1 value");
        if (msg.value > betValue) {
            msg.sender.transfer(msg.value - betValue);
        }
        isPlaying[msg.sender] = true;
        playerToGame[msg.sender] = gameID;
    }

    function endGame(string calldata gameID, bool isWon)
        external
        onlyPlaying(gameID)
    {
        if (isWon) {
            require(address(this).balance >= 2*betValue, "insufficient balance");
            msg.sender.transfer(2*betValue);
        }
        isPlaying[msg.sender] = false;
        playerToGame[msg.sender] = "";
    }

    function compareStringsbyBytes(string memory s1, string memory  s2)
        public
        pure
        returns(bool)
    {
        return keccak256(abi.encodePacked(s1)) == keccak256(abi.encodePacked(s2));
    }

    function () external payable {}
}