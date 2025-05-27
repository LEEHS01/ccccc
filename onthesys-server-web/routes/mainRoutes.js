const express = require('express');
const router = express.Router();
const userController = require('../controllers/mainController.js');
const option = require('../system/option.js')

const cors = require('cors');

router.use(cors());
router.use(express.static(option.buildPath));
router.get('/', userController.navigateWingl);

module.exports = router