
const path = require('path');
const option = require('../system/option');


exports.navigateWingl = (req, res) => {
    res.sendFile(path.join(option.buildPath, 'index.html'));
}