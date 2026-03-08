#define PRA_EXPORTS
#include "pch.h"
#include "pra_exports.h"

#include <cstring>
#include <string>

double pra_test_add(double a, double b)
{
    return a + b;
}

double pra_calc_equity_vs_weighted_range(
    const char* hero_hand_class,
    const char* villain_weighted_range,
    int iterations,
    int* error_code)
{
    if (error_code != nullptr)
        *error_code = 0;

    if (hero_hand_class == nullptr || villain_weighted_range == nullptr)
    {
        if (error_code != nullptr)
            *error_code = 1;
        return -1.0;
    }

    if (iterations <= 0)
    {
        if (error_code != nullptr)
            *error_code = 2;
        return -1.0;
    }

    // Temporary stub so the full interop contract can be tested.
    // Replace this with OMPEval-backed logic next.
    return 0.5;
}