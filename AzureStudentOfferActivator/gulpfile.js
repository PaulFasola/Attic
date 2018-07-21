var gulp = require('gulp');
var less = require('gulp-less');
var path = require('path');

gulp.task('default', function(){
  return gulp.start('lessify', 'less-to-css');
});

gulp.task('lessify', function() {
  return gulp.src('./Assets/**/*.less')
    .pipe(less({
      paths: [ path.join(__dirname, 'less', 'includes') ]
    }))
    .pipe(gulp.dest('./Assets/'));
});

gulp.task('less-to-css', function() {
  return gulp.watch([
    './Assets/**/*.less',
  ], ['lessify']);
});