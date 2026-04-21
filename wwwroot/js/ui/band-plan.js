// Band segment data for UK and USA band plans.
// Frequencies in Hz. Each entry gives the representative dial frequency and
// the mode string used by this app (matches CatMessageDispatcher mode names).
// 60m is a special case: no segments, just named channels with USB.

export const BAND_PLANS = {
    UK: {
        '160m': {
            CW:   { freq: 1820000,  mode: 'CW-U',   label: 'CW' },
            FT8:  { freq: 1840000,  mode: 'DATA-U',  label: 'FT8' },
            SSB:  { freq: 1850000,  mode: 'LSB',     label: 'SSB' }
        },
        '80m': {
            CW:   { freq: 3520000,  mode: 'CW-U',   label: 'CW' },
            FT8:  { freq: 3573000,  mode: 'DATA-U',  label: 'FT8' },
            RTTY: { freq: 3580000,  mode: 'RTTY-L',  label: 'RTTY' },
            SSB:  { freq: 3680000,  mode: 'LSB',     label: 'SSB' }
        },
        '60m': {
            // UK 60m channels - display frequency in USB (Ofcom/IARU R1 allocation)
            CH1:  { freq: 5261500,  mode: 'USB', label: '5.262' },
            CH2:  { freq: 5280000,  mode: 'USB', label: '5.280' },
            CH3:  { freq: 5288500,  mode: 'USB', label: '5.289' },
            CH4:  { freq: 5298500,  mode: 'USB', label: '5.299' },
            CH5:  { freq: 5313000,  mode: 'USB', label: '5.313' },
            CH6:  { freq: 5333000,  mode: 'USB', label: '5.333' },
            CH7:  { freq: 5354000,  mode: 'USB', label: '5.354' },
            CH8:  { freq: 5362000,  mode: 'USB', label: '5.362' },
            CH9:  { freq: 5373000,  mode: 'USB', label: '5.373' },
            CH10: { freq: 5405000,  mode: 'USB', label: '5.405' }
        },
        '40m': {
            CW:   { freq: 7020000,  mode: 'CW-U',   label: 'CW' },
            FT8:  { freq: 7074000,  mode: 'DATA-U',  label: 'FT8' },
            RTTY: { freq: 7040000,  mode: 'RTTY-L',  label: 'RTTY' },
            SSB:  { freq: 7160000,  mode: 'LSB',     label: 'SSB' }
        },
        '30m': {
            // No phone (SSB) allocation on 30m
            CW:   { freq: 10115000, mode: 'CW-U',   label: 'CW' },
            FT8:  { freq: 10136000, mode: 'DATA-U',  label: 'FT8' }
        },
        '20m': {
            CW:   { freq: 14025000, mode: 'CW-U',   label: 'CW' },
            FT8:  { freq: 14074000, mode: 'DATA-U',  label: 'FT8' },
            RTTY: { freq: 14080000, mode: 'RTTY-U',  label: 'RTTY' },
            SSB:  { freq: 14225000, mode: 'USB',     label: 'SSB' }
        },
        '17m': {
            CW:   { freq: 18080000, mode: 'CW-U',   label: 'CW' },
            FT8:  { freq: 18100000, mode: 'DATA-U',  label: 'FT8' },
            SSB:  { freq: 18130000, mode: 'USB',     label: 'SSB' }
        },
        '15m': {
            CW:   { freq: 21025000, mode: 'CW-U',   label: 'CW' },
            FT8:  { freq: 21074000, mode: 'DATA-U',  label: 'FT8' },
            RTTY: { freq: 21080000, mode: 'RTTY-U',  label: 'RTTY' },
            SSB:  { freq: 21280000, mode: 'USB',     label: 'SSB' }
        },
        '12m': {
            CW:   { freq: 24895000, mode: 'CW-U',   label: 'CW' },
            FT8:  { freq: 24915000, mode: 'DATA-U',  label: 'FT8' },
            SSB:  { freq: 24940000, mode: 'USB',     label: 'SSB' }
        },
        '10m': {
            CW:   { freq: 28025000, mode: 'CW-U',   label: 'CW' },
            FT8:  { freq: 28074000, mode: 'DATA-U',  label: 'FT8' },
            RTTY: { freq: 28080000, mode: 'RTTY-U',  label: 'RTTY' },
            SSB:  { freq: 28500000, mode: 'USB',     label: 'SSB' }
        },
        '6m': {
            CW:   { freq: 50050000, mode: 'CW-U',   label: 'CW' },
            FT8:  { freq: 50313000, mode: 'DATA-U',  label: 'FT8' },
            SSB:  { freq: 50150000, mode: 'USB',     label: 'SSB' }
        },
        '4m': {
            // UK only - 70 MHz band
            CW:   { freq: 70050000, mode: 'CW-U',   label: 'CW' },
            FT8:  { freq: 70154000, mode: 'DATA-U',  label: 'FT8' },
            SSB:  { freq: 70200000, mode: 'USB',     label: 'SSB' }
        }
    },
    USA: {
        '160m': {
            CW:   { freq: 1820000,  mode: 'CW-U',   label: 'CW' },
            FT8:  { freq: 1840000,  mode: 'DATA-U',  label: 'FT8' },
            SSB:  { freq: 1850000,  mode: 'LSB',     label: 'SSB' }
        },
        '80m': {
            CW:   { freq: 3510000,  mode: 'CW-U',   label: 'CW' },
            FT8:  { freq: 3573000,  mode: 'DATA-U',  label: 'FT8' },
            SSB:  { freq: 3800000,  mode: 'LSB',     label: 'SSB' }
        },
        '60m': {
            // USA 60m channels (USB, 15W max EIRP on some, 100W on others per FCC Part 97)
            CH1:  { freq: 5330500,  mode: 'USB', label: '5.331' },
            CH2:  { freq: 5346500,  mode: 'USB', label: '5.347' },
            CH3:  { freq: 5357000,  mode: 'USB', label: '5.357' },
            CH4:  { freq: 5371500,  mode: 'USB', label: '5.372' },
            CH5:  { freq: 5403500,  mode: 'USB', label: '5.404' }
        },
        '40m': {
            CW:   { freq: 7010000,  mode: 'CW-U',   label: 'CW' },
            FT8:  { freq: 7074000,  mode: 'DATA-U',  label: 'FT8' },
            RTTY: { freq: 7080000,  mode: 'RTTY-L',  label: 'RTTY' },
            SSB:  { freq: 7200000,  mode: 'LSB',     label: 'SSB' }
        },
        '30m': {
            CW:   { freq: 10115000, mode: 'CW-U',   label: 'CW' },
            FT8:  { freq: 10136000, mode: 'DATA-U',  label: 'FT8' }
        },
        '20m': {
            CW:   { freq: 14025000, mode: 'CW-U',   label: 'CW' },
            FT8:  { freq: 14074000, mode: 'DATA-U',  label: 'FT8' },
            RTTY: { freq: 14080000, mode: 'RTTY-U',  label: 'RTTY' },
            SSB:  { freq: 14225000, mode: 'USB',     label: 'SSB' }
        },
        '17m': {
            CW:   { freq: 18080000, mode: 'CW-U',   label: 'CW' },
            FT8:  { freq: 18100000, mode: 'DATA-U',  label: 'FT8' },
            SSB:  { freq: 18130000, mode: 'USB',     label: 'SSB' }
        },
        '15m': {
            CW:   { freq: 21025000, mode: 'CW-U',   label: 'CW' },
            FT8:  { freq: 21074000, mode: 'DATA-U',  label: 'FT8' },
            RTTY: { freq: 21080000, mode: 'RTTY-U',  label: 'RTTY' },
            SSB:  { freq: 21300000, mode: 'USB',     label: 'SSB' }
        },
        '12m': {
            CW:   { freq: 24895000, mode: 'CW-U',   label: 'CW' },
            FT8:  { freq: 24915000, mode: 'DATA-U',  label: 'FT8' },
            SSB:  { freq: 24940000, mode: 'USB',     label: 'SSB' }
        },
        '10m': {
            CW:   { freq: 28025000, mode: 'CW-U',   label: 'CW' },
            FT8:  { freq: 28074000, mode: 'DATA-U',  label: 'FT8' },
            RTTY: { freq: 28080000, mode: 'RTTY-U',  label: 'RTTY' },
            SSB:  { freq: 28500000, mode: 'USB',     label: 'SSB' }
        },
        '6m': {
            CW:   { freq: 50050000, mode: 'CW-U',   label: 'CW' },
            FT8:  { freq: 50313000, mode: 'DATA-U',  label: 'FT8' },
            SSB:  { freq: 50150000, mode: 'USB',     label: 'SSB' }
        }
        // No 4m allocation in USA
    }
};

export function getSegments(bandPlan, band) {
    const plan = BAND_PLANS[bandPlan] || BAND_PLANS['UK'];
    return plan[band] || null;
}
