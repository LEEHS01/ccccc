const express = require('express')
const authController = require('../controllers/authController')
const router = express.Router();

router.use(express.json());
router.post('/certification', authController.certification);
router.post('/validation', authController.validation)

module.exports = router