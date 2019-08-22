pragma solidity 0.5.0;

contract SheepFaucet {
    mapping (address => int) public claimCount;

    function claimFaucet()
        external
    {
        require(claimCount[msg.sender] < 3, "maximum claim exceed");
        require(address(this).balance > 5 ether, "insufficient balance");
        claimCount[msg.sender] += 1;
        msg.sender.transfer(5 ether);
    }

    function () external payable {}
}