const fs = require('fs');
const path = require('path');
const contracts = path.resolve(__dirname, '../build/contracts/');
const unityAbis = path.resolve(__dirname, '../../Assets/Contracts/');

module.exports = async function() {
  let builtContracts = fs.readdirSync(contracts);
  builtContracts.forEach((contract) => {
    if (contract === 'Migrations.json') return;
    const name = contract.split('.')[0];
    let json = JSON.parse(fs.readFileSync(path.resolve(contracts, contract)));
    let { abi, networks } = json;
    if (!Object.keys(networks).length) return;
    fs.writeFileSync(path.resolve(unityAbis, contract), JSON.stringify(json.abi));
    fs.writeFileSync(path.resolve(unityAbis, name + 'Address.txt'), networks['89'].address);
  });
};
