#define PRA_EXPORTS
#include "pra_exports.h"

#include <omp/EquityCalculator.h>
#include <omp/CardRange.h>

#include <string>
#include <vector>
#include <exception>

double pra_calc_equity_vs_weighted_range(
    const char* hero_hand_class,
    const char* villain_weighted_range,
    int iterations,
    int* error_code)
{
    if (error_code != nullptr)
        *error_code = 0;

    try
    {
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

        std::string hero(hero_hand_class);
        std::string villain(villain_weighted_range);

        std::vector<omp::CardRange> ranges;
        ranges.emplace_back(hero);
        ranges.emplace_back(villain);

        omp::EquityCalculator calculator;
        calculator.setHandLimit(static_cast<uint64_t>(iterations));

        const bool started = calculator.start(
            ranges,
            0,          // boardCards
            0,          // deadCards
            false,      // enumerateAll -> Monte Carlo
            0.0,        // stdevTarget -> unused with hand limit
            nullptr,    // callback
            0.2,        // updateInterval
            0);         // threadCount -> auto

        if (!started)
        {
            if (error_code != nullptr)
                *error_code = 3;
            return -1.0;
        }

        calculator.wait();
        auto results = calculator.getResults();

        if (results.players < 1)
        {
            if (error_code != nullptr)
                *error_code = 4;
            return -1.0;
        }

        return results.equity[0];
    }
    catch (...)
    {
        if (error_code != nullptr)
            *error_code = 99;
        return -1.0;
    }
}