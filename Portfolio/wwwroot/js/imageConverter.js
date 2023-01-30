'use strict';
//declare function to handle conversion
const base64ToBlob = (base64Str, contentType = '', sliceSize = 512) => {
  //decode base 64 string into string of coded characters
  const byteCharacters = atob(base64Str);

  const byteArrays = [];
  for (let offset = 0; offset < byteCharacters.length; offset += sliceSize) {
    //get a slice of byte characters (512 bytes worth)
    const slice = byteCharacters.slice(offset, offset + sliceSize);

    //create array of byte values that is the size of the slice
    const byteNumbers = new Array(slice.length);

    //fill byte value array with slice
    for (let i = 0; i < slice.length; i++) {
      byteNumbers[i] = slice.charCodeAt(i);
    }

    //create uint8 array out of byte value array
    const byteArray = new Uint8Array(byteNumbers);

    //push result to byte array
    byteArrays.push(byteArray);
  }

  //create blob out of byteArray
  const blob = new Blob(byteArrays, {
    type: contentType,
  });

  //return result
  return blob;
};
