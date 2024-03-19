#include <stdio.h>
#define PCRE2_CODE_UNIT_WIDTH 8
#include <pcre2.h>
#include <string.h>

int
main(int argc, char *argv[])
{

  if(argc < 3)
  {
    fprintf(stderr,"usage: %s \"regex\" subject\n",argv[0]);
    return EXIT_FAILURE;
  }

  /* for pcre2_compile */
  pcre2_code_8 *re;
  PCRE2_SIZE erroffset;
  int errcode;
  PCRE2_UCHAR8 buffer[128];

  /* for pcre2_match */
  int rc;
  PCRE2_SIZE* ovector;

  const char *pattern = argv[1];
  size_t pattern_size = strlen(pattern);

  const char *subject = argv[2];
  size_t subject_size = strlen(subject);
  uint32_t options = 0;

  pcre2_match_data_8 *match_data;
  uint32_t ovecsize = 128;

  re = pcre2_compile_8(pattern, pattern_size, options, &errcode, &erroffset, NULL);
  if (re == NULL)
  {
    pcre2_get_error_message_8(errcode, buffer, 120);
    fprintf(stderr,"%d\t%s\n", errcode, buffer);
    return 1;
  }

  match_data = pcre2_match_data_create_8(ovecsize, NULL);
  rc = pcre2_match_8(re, subject, subject_size, 0, options, match_data, NULL);
  if(rc == 0) {
    fprintf(stderr,"offset vector too small: %d",rc);
  }
  else if(rc > 0)
  {
    printf("Match\n");
    // ovector = pcre2_get_ovector_pointer_8(match_data);
    // PCRE2_SIZE i;
    // for(i = 0; i < rc; i++)
    // {
    //   PCRE2_SPTR8 start = subject + ovector[2*i];
    //   PCRE2_SIZE slen = ovector[2*i+1] - ovector[2*i];
    //   printf( "%2d: %.*s\n", (int)i, (int)slen, (char *)start );
    // }
  }
  else if (rc < 0)
  {
    printf("No match\n");
  }

  pcre2_match_data_free_8(match_data);
  pcre2_code_free_8(re);

  return 0;
}