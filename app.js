
/**
 * Module dependencies.
 */

var express = require('express')
  , routes = require('./routes');

var app = module.exports = express.createServer();

// Configuration

app.configure(function(){
  app.set('views', __dirname + '/views');
  app.set('view engine', 'jade');
  app.use(express.bodyParser());
  app.use(express.methodOverride());
  app.use(app.router);
  app.use(express.static(__dirname + '/public'));
});

app.configure('development', function(){
  app.use(express.errorHandler({ dumpExceptions: true, showStack: true }));
});

app.configure('production', function(){
  app.use(express.errorHandler());
});

// Routes


app.get('/', function(res,res){
	var shSyntaxHighlighter = require('shCore').SyntaxHighlighter;
	var shJScript = require('shBrushJScript').Brush;
	var code = '\
		function helloWorld()\
		{\
			// this is great!\
			for(var i = 0; i <= 1; i++)\
				alert("yay");\
		}\
		';
	var brush = new shJScript();
	res.render('index',{content:brush.getHtml(code)});
})

app.listen(3000, function(){
  console.log("Express server listening on port %d in %s mode", app.address().port, app.settings.env);
});


