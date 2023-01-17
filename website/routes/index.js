var express = require('express');
var router = express.Router();

/* GET home page. */
router.get('/', function(req, res, next) {
  res.render('index', { title: 'Statistics', listenerData: '12, 19, 3, 5, 10' });
});

module.exports = router;
