#include <stdio.h>
#include <stdlib.h>
#include <regex.h>

int main(int argc, char *argv[])
{
    if(argc < 3)
    {
        fprintf(stderr,"usage: %s \"regex\" subject\n",argv[0]);
        return EXIT_FAILURE;
    }
    
    char *regex_pattern = argv[1];
    regex_t regex;
    int ret;

    // Compile the regular expression
    ret = regcomp(&regex, regex_pattern, REG_EXTENDED);
    if (ret != 0) {
        printf("Error compiling regex\n");
        exit(1);
    }

    // Match the regular expression against a string
    char *test_string = argv[2];
    ret = regexec(&regex, test_string, 0, NULL, 0);
    if (ret == 0) {
        printf("P");
    } else if (ret == REG_NOMATCH) {
        printf("N");
    } else {
        printf("E");
    }

    // Free the compiled regex
    regfree(&regex);

    return 0;
}
