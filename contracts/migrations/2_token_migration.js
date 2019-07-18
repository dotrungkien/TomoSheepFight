const ERC20 = artifacts.require('ERC20');

module.exports = function(deployer) {
  deployer.deploy(ERC20, '100000000000');
};
