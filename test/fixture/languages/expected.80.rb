# Ruby uses '#' for line comments

#This is a comment with no space after the '#'

# This is a comment that need to have enough text to span over two lines. It
# doesn't yet; ok now it does.

# This is a two-line comment made up of two single-line comments.

    # This is a comment that's been indented
puts "Some Ruby code"

# This is a line comment right before a multi-line comment
=begin
This is a multi-line comment. Now some random text that will make the comment
large enough to fill a couple of lines at least. Btw, the '=begin' and '=end'
have to be at the start of the line with no indents.
=end
# This is a comment right after a multi-line comment