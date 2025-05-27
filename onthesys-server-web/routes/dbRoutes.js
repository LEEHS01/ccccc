const express = require('express')
const router = express.Router();
const option = require('../system/option')

const sql = require('mssql');
const dbController = require('../controllers/dbController')

router.use(express.json());
router.post('/', dbController.Query);

module.exports = router