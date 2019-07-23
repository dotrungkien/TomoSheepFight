pragma solidity 0.4.24;
pragma experimental ABIEncoderV2;

contract SheepFight {
    mapping (address => bool) public isPlaying;

    uint public betValue = 1 ether;

    modifier onlyPlaying()
    {
        require(isPlaying[msg.sender], 'player must be in game');
        _;
    }

    function play()
        external
        payable
        returns (bytes32)
    {
        require(msg.value >= betValue, 'must bet 1 value');
        if (msg.value > betValue) {
            msg.sender.transfer(msg.value - betValue);
        }
        isPlaying[msg.sender] = false;
        return keccak256(
            abi.encodePacked(blockhash(block.number - 1), msg.sender)
        );
    }

    function endGame(bool isWon)
        external
        onlyPlaying()
    {
        if (isWon) {
            require(address(this).balance >= 2*betValue, 'insufficient balance');
            msg.sender.transfer(2*betValue);
        }
        isPlaying[msg.sender] = false;
    }

    function () public payable {}
}