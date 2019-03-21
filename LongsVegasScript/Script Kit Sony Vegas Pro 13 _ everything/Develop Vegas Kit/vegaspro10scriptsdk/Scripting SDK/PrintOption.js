/**
 * This script (along with its configuration file) demonstrates use of
 * the JScript print function.  The print function provides a simple
 * way for a script to output lines of text to a file.
 *
 * To use the print function, Vegas must first tell the JScript engine
 * the name of the file where the text is written.  In the
 * configuration file (PrintOption.js.config), you will find an
 * element named "PrintOption" whose inner text contains the name of
 * the output file.  The inner text of the PrintOption element can
 * either be a full path or relative to the directory containing the
 * script.
 *
 * Revision Date: Apr. 20, 2004.
 **/

import Sony.Vegas;

print("this is a test");
print("Vegas.Version == " + Vegas.Version);
