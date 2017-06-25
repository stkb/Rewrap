# Html #

> language: "html"

Html (includes Xml) has its own wrapping behaviors. Unlike other source code
files, all content is considered equal when it comes to selections; selecting a
whole document will wrap comments and other content alike.

    <!-- comment comment -->              <!-- comment
    text text text text           ->      comment -->
                ¦                         text text
                ¦                         text text

Mixed content on a line is wrapped as one.

    <a>one two</a>      ->      <a>one
             ¦                  two</a>

Any line that starts with a tag starts a new paragraph.

    Foo bar      ->      Foo   ¦
    <div> ¦              bar   ¦
          ¦              <div> ¦

Any line that ends with a tag ends the paragraph.

    <div>     ¦               <div>     ¦
    aaa bbbbbbb      ->       aaa       ¦
    cc                        bbbbbbb cc¦

This means things like this are preserved.

    <ul>                 ¦              <ul>
      <li> 1 </li>       ¦      ->        <li> 1 </li>
      <li> 2 </li>       ¦                <li> 2 </li>
    </ul>                ¦              </ul>


## Embedded JS and CSS

If the selection within an HTML document contains script or style tags, the
contents of those tags are wrapped using the rules for javascript or css.

    <script>       ¦        ->      <script>       ¦
      // one two three                // one two   ¦
      // four      ¦                  // three four¦
      var s = "string";               var s = "string";
    </script>      ¦                </script>      ¦

    <style>        ¦        ->      <style>        ¦
      /* one two three                /* one two   ¦
      four */      ¦                  three four */¦
      .warn {      ¦                  .warn {      ¦
        color: red;¦                    color: red;¦
        font-weight: bold;              font-weight: bold;
      }            ¦                  }            ¦
    </style>       ¦                </style>       ¦

    <notscript>    ¦        ->      <notscript>    ¦
      // one two three                // one two   ¦
      // four      ¦                  three // four¦
    </notscript>   ¦                </notscript>   ¦

