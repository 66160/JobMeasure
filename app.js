/**
 * Module dependencies.
 */

var express = require('express')
  , routes = require('./routes');

var app = module.exports = express.createServer();
var fs = require('fs'),
	path = require('path'),
	jq = require('jquery'),
	mongo = require('mongodb'),
	server = new mongo.Server('localhost', 27017, {auto_reconnect: true}),
	db = new mongo.Db('css', server);

// Configuration

app.configure(function(){
  app.use(express.cookieParser());
  app.use(express.session({ secret: "ryu" }));

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

app.get('/', function(req, res) {
	res.redirect('/login');
});

app.get('/register', function(req, res) {
	res.render('register', {title: 'Register'});
});

app.post('/register', function(req, res) {
	var user = req.body.user,
		pwd = req.body.pwd;
	
	openColc('users', function(err, col) {
		if (hasErr(err)) return;

		col.find({_id: user}).toArray(function(err, item) {
			if (hasErr(err)) return;

			if (item.length == 0) {
				col.insert({_id: user, pwd: pwd});
				req.session.user_id = user;
				res.redirect('/projs');
			}
			else
				res.redirect('/register');
		});
	});
});

app.get('/debug/clearusers', function(req, res) {
	openColc('users', function(err, col) {
		if (hasErr(err)) return;
		console.log('clear users');
		col.remove({});
		res.end('users cleared');
	});
});

app.get('/login', function(req, res) {
	res.render('login', {title: 'login'});
});

app.post('/login', function(req, res) {
	var user = req.body.user,
		pwd = req.body.pwd;
	
	openColc('users', function(err, col) {
		if (hasErr(err)) return;

		col.find({_id: user}).toArray(function(err, item) {
			if (hasErr(err)) return;

			if (item.length == 1 && item[0].pwd == pwd) {
				req.session.auth = true;
				res.redirect('/projs');
			}
			else
				res.redirect('/login');
		});
	});
});

app.get('/projs', function(req, res) {
	res.end('This is project page.');
});

app.get('/cs/*', function(req, res) {
	var file = req.params[0];
	readCs(file, function(p, cs) {
		if (cs != null) {
			var filename = path.basename(file);
			res.render('showcs', {title: filename, cs: cs, his: '/history/' + file});
		} else {
			console.log("Cannot find " + p);
			res.end("Cannot find " + file);
		}
	});
});	

app.get('/comp/:file1/:file2', function(req, res) {
	var files = [req.params.file1, req.params.file2];
	readCs(files[0], function(p1, cs1) {
		readCs(files[1], function(p2, cs2) {
			if (cs1 != null && cs2 != null)
				res.render('index', {title: files.join(' vs. '), left: cs1, right: cs2});
			else {
				var paths = [p1, p2];
				console.log("Cannot find " + paths.join(', '));
				res.end("Cannot find " + files.join(', '));
			}
		});
	});
});
app.get('/diff/*', function(req, res) {
	var file = req.params[0];
	var files = parseFiles(file);
	if (files.length > 1) {
		diffFile(files, 0, [], function(diffs) {
			res.render('showdiff', {
				title: 'diff', 
				diffs: diffs, 
			});
		});
	} else {
		openColc('histories', function(err, col) {
			if (hasErr(err)) return;

			col.find({_id: removeLastVer(file)}).toArray(function(err, item) {
				if (hasErr(err)) return;

				diffFile(files, 0, [], function(diffs) {
					res.render('showdiff', {
						title: 'diff', 
						diffs: diffs, 
						his: item[0].his
					});
				});
			});
		});
	}
});

app.get('/diffcs/*', function(req, res) {
	var file = req.params[0];
	readFile(file, function(p, cs) {
		if (cs != null) {
			res.render('showcs', {title: file, cs: cs});
		} else {
			console.log("Cannot find " + p);
			res.end("Cannot find " + file);
		}
	});
});	

app.get('/list', function(req, res) {
	res.redirect('/list/css');
});

app.get('/list/*', function(req, res) {
	ssdir(req.params, function(result) {
		res.render('list', {
			title: 'list',	
			columns: ["isfolder", "name"],
			data: splitResult('/list/' + req.params, result),
			his: '/history/' + req.params
		});
	});
});

app.get('/history', function(req, res) {
	res.redirect('/history/css');
});

app.get('/history/*', function(req, res) {
	var file = req.params[0];
	openColc('histories', function(err, col) {
		if (hasErr(err)) return;

		col.find({_id: file}).toArray(function(err, item) {
			if (hasErr(err)) return;

			if (item.length == 1)
				renderHis(res, item[0].his);
			else {
				sshis(file, function(result) {
					parseHistory(result, function(items) {
						var his = combineHistory('/diff/' + file, items);
						col.insert({_id:file, his: his});
						renderHis(res, his);
					});
				});
			}
		});
	});
});

app.get('/db', function(req, res) {
	var Server = mongo.Server,
		Db = mongo.Db;

	var server = new Server('localhost', 27017, {auto_reconnect: true});
	var db = new Db('exampleDb', server);
	db.open(function(err, db) {
		if (err) {
			console.log(err);
			return;
		}

		db.collection('test', function(err, col) {
			var docs = [{mykey:1}, {mykey:2}, {mykey:3}];
			col.insert(docs, {safe:true}, function(err, result) {
				var stream = col.find().streamRecords();
				stream.on('data', function(item){
					res.write(item.mykey.toString());
				});
				stream.on('end', function() {
					res.end();
				});
			});
		});
	});
});


app.listen(3000, function() {
  console.log("Express server listening on port %d in %s mode", app.address().port, app.settings.env);
});

// lib

function login(user, pwd) {
	
}

function removeLastVer(file) {
	var pos = file.lastIndexOf(':');
	if (pos != -1)
		return file.substring(0, pos);
	else
		return file;
}

function renderHis(res, his) {
	res.render('history', {
		title: '履歴', 
		histories: his
	});
}

function openColc(colc, callback) {
	openDb(function(err, db) {
		if (err)
			callback(err, null);
		else
			db.collection(colc, function(err, colc) {
				callback(err, colc);
			});
	});
}

function openDb(callback) {
	if (db._state == 'connected')
		callback(null, db);
	else
		db.open(function(err, db) { 
			callback(err, db);
	   	});
}

function hasErr(err) {
	if (err) {
		console.log(err);
		return true;
	}
	return false;
}

function diffFile(files, id, diffs, callback) {
	var file = files[id];
	diff(file.filename, file.version, function(result) {
		diffs.push(createDiff(file, result));
		if (id == files.length - 1)
			callback(diffs);
		else 
			diffFile(files, ++id, diffs, callback);
	});
}

function createDiff(file, result) {
	var filename = path.basename(file.filename) + '(' + file.version + ' vs. ' + (file.version - 1) + ')';
	var url = file.filename + '.' + file.version;
	return { filename: filename, url: '/diffcs/' + url, diff: result };
}

function parseFiles(fileurl) {
	console.log(fileurl);
	var files = [],
		file,
		folder = path.dirname(fileurl),
		filenames = path.basename(fileurl)
		parts = filenames.split(':');

	for(var i in parts) {
		var part = parts[i];
		if (i % 2 == 0)
			file = { filename: path.join(folder, part), version: 0 };
		else {
			var num = parseInt(part);
			if (!isNaN(num))
				file.version = num;

			files.push(file);
		}
	}

	return files;
}

function combineHistory(base, items) {
	var dicDate = {},
		dicTimeComment;

	items.forEach(function(item) {
		if (!item.isauto) {
			var date = item.date;
			var dtkey = date.getFullYear() + '/' + (date.getMonth() + 1) + '/' + date.getDate();
			if (dtkey in dicDate)
				dicTimeComment = dicDate[dtkey];
			else {
				dicTimeComment = {};
				dicDate[dtkey] = dicTimeComment;
			}

			var tuckey = date.getTime() + '|' + item.user + '|' +  item.comment.join('\n');
			if (tuckey in dicTimeComment)
				dicTimeComment[tuckey].push(item);
			else
				dicTimeComment[tuckey] = [item];
		}
	});

	var histories = [];
	for(var date in dicDate) {
		histories.push({ date: date, cms: getComments(base, dicDate[date]) });
	}
	console.log('combineHistory> histories.lenght=' + histories.length);
	return histories;
}

function getComments(base, dictuc) {
	var cms = [];
	for(var tuc in dictuc) {
		var items = dictuc[tuc],
			item = items[0];

		if (noComment(item.comment)) {
			item.comment = [];
			item.comment.push('なし');
		}
		cms.push({ 
			comment: item.comment, 
			user: item.user, 
			date: item.date, 
			fileurl: getFileUrl(base, items) });
	}
	return cms;
}

function noComment(comment) {
	if (comment) {
		if (comment.length > 0) {
			for(var i in comment) {
				var cm = comment[i];
				if (cm)
					return false;
			}
		}
	}

	return true;
}

function getFileUrl(base, items) {
	if (items.length == 1) {
		var it = items[0];
		if (!it.filename)
			return base + ':' + it.version; 
	}
	
	var url = base + '/';
	for(var i in items) {
		var it = items[i];
		if (i > 0)
			url += ':';
		url += it.filename + ':' + it.version;
	}
	return url;
}

function parseHistory(content, callback) {
	var lines = content.split('\r\n'),
		items = [],
		item,
		isfile = false,
		isver = false;

	for(var i in lines) {
		var line = lines[i];

		var version = getBetween(line, '*****************', '*****************');
		if (version) {
			item = createHistoryItem();
			item.isauto = true;
			item.version = parseVersion(version);
			items.push(item);

			isfile = false;
			isver = true;
			continue;
		}

		var filename = getBetween(line, '*****', '*****');
		if (filename) {
			item = createHistoryItem();
			item.filename = filename;
			items.push(item);

			isfile = true;
			isver = false;
			continue;
		}

		if (isfile) 
			parseFileBlock(item, line);

		if (isver)
			parseVersionBlock(item, line);

	}
	callback(items);
}

function parseFileBlock(item, line) {
	if (item.comment && line)
		item.comment.push(line);

	if (startWith(line, 'ユーザー:'))
		parseUserAndDate(item, line);

	if (startWith(line, 'コメント:')) {
		item.comment = [];
		item.comment.push(getAfterAndTrim(line, 'コメント:'));
	}

	if (startWith(line, 'バージョン')) {
		var numstr = getAfterAndTrim(line, 'バージョン');
		var num = parseInt(numstr);
		if (!isNaN(num))
			item.version = num;	
	}
}

function parseVersionBlock(item, line) {
	if (item.comment && line)
		item.comment.push(line);

	if (startWith(line, 'ユーザー:'))
		parseUserAndDate(item, line);

	if (startWith(line, 'コメント:')) {
		item.isauto = false;
		item.comment = [];
		item.comment.push(getAfterAndTrim(line, 'コメント:'));
	}
}

function getAfterAndTrim(str, keyword) {
	var valstr = str.substring(keyword.length);
	return jq.trim(valstr);
}

function startWith(str, keyword) {
	var pos = str.indexOf(keyword); 
	return pos == 0;
}	

function createHistoryItem() {
	return { filename: null, version: 0, user: null, date: null, comment:null, isauto: false };
}

function parseVersionFilename(line) {
	var end = line.lastIndexOf(' ');
	if (end != -1)
		return line.substring(0, end);

	return null;
}

function parseUserAndDate(item, line) {
	var values = getPartValues(line);
	if (values && values.length == 3) {
		item.user = values[0];
		item.date = toDate(values[1], values[2]);
	}
}

function getPartValues(line) {
	var values = [],
		parts = line.split(' ');
	for(var i in parts) {
		var part = parts[i];
		if (part)
			if (part.indexOf(':') != (part.length - 1))
				values.push(part);
	}
	return values;
}

function toDate(date, time) {
	var d = '20' + date,
		t = time + ':00 GMT+0900',
		str = d + ' ' + t;

	return new Date(str);	
}

function parseVersion(version) {
	var parts = version.split(' ');
	if (parts && parts.length == 2) {
		var ver = parseInt(parts[1]);
		if (!isNaN(ver))
			return ver;
	}
	return 0;
}

function getBetween(str, start, end) {
	var b = str.indexOf(start);
	if (b == -1) return null;

	b += start.length;
	if (b >= str.length) return null;

	var e = str.indexOf(end, b);
	if (e == -1) return null;

	return jq.trim(str.substring(b, e));
}

function splitResult(base, result) {
	var items = [];
	var isfirst = true;
	var parts = result.split('\r\n');
	for (i = 1; i < parts.length; i++) {
		var part = parts[i];
		if (!part) break;
		
		var isfolder = false;
		var name = part;
		if (part.substring(0,1) == '$') {
			isfolder = true;
			name = part.substring(1);
		}

		if (!isfolder)
			base = base.replace('/list', '/cs');

		var item = {
			isfolder: isfolder,
			name: '<a href="' + base + '/' + name + '">' + name + '</a>'
		};
		items.push(item);
	}

	return items;
}

function readCs(file, callback) {
	ssget(file, function(result) {
		var filename = path.basename(file);
		var p = csp(filename);
		fs.readFile(p, function(err, data) {
			if (err) throw err;
			callback(p, data);
		});
	});
}

function readFile(file, callback) {
	var filename = path.basename(file);
	var p = csp(filename);
	fs.readFile(p, function(err, data) {
		if (err) throw err;
		callback(p, data);
	});
}

function diff(file, version, callback) {
	ssgetv(file, version, function(result) {
		var filename = path.basename(file);
		var p = csp(filename);
		var pnew = p + '.' + version;
		fs.rename(p, pnew, function() {
			var oldver = version - 1;
			var pold = p + '.' + oldver;
			ssgetv(file, oldver, function(result) {
				fs.rename(p, pold, function() {
					diffCs(pold, pnew, callback);
				});
			});
		});
	});
}

function diffCs(file1, file2, callback) {
	exe(binp('localdiff.exe'), ['-u', file1, file2], callback);
}

function ssdir(sub, callback) {
	ss('Dir', sub, callback);
}

function ssget(sub, callback) {
	ss('Get', sub, callback);
}

function ssstat(sub, callback) {
	ss('Status', sub, callback);
}

function sshis(sub, callback) {
	ss('History', sub, callback);
}

function ssgetv(sub, version, callback) {
	process.env.ssdir = '\\\\src\\VSS\\CSS';
	process.env.ssuser = 'ryu';
	process.env.sspwd = 'ryu';
	exe(ssp('ss.exe'), ['Get', '-V' + version, '$/' + sub], callback);
}

function ss(cmd, sub, callback) {
	process.env.ssdir = '\\\\src\\VSS\\CSS';
	process.env.ssuser = 'ryu';
	process.env.sspwd = 'ryu';
	exe(ssp('ss.exe'), [cmd, '$/' + sub], callback);
}

function exe(exefullpath, args, callback) {
	var cmd = '"' + binp('utf8wrapper.exe') + '"';
	cmd += ' "' + exefullpath + '"';
	for(i = 0; i < args.length; i++) {
		cmd += ' ' + args[i];
	}
	console.log('exe>' + cmd);

	var forker = require('child_process');
	forker.exec(cmd, { cwd: csp() }, function(err, outstr) {
		callback(outstr);
	});
}

function csp(filename) {
	return path.join(__dirname, 'cs', filename);
}
function binp(filename) {
	return path.join(__dirname, 'bin', filename);
}
function ssp(filename) {
	return path.join('c:\\Program Files\\Microsoft Visual SourceSafe', filename);
}



