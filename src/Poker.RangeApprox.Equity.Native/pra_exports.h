#pragma once

#ifdef PRA_EXPORTS
#define PRA_API extern "C" __declspec(dllexport)
#else
#define PRA_API extern "C" __declspec(dllimport)
#endif

PRA_API double pra_test_add(double a, double b);

PRA_API double pra_calc_equity_vs_weighted_range(
    const char* hero_hand_class,
    const char* villain_weighted_range,
    int iterations,
    int* error_code);