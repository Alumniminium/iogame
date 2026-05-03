/**
 * Catppuccin Frappe Color Theme
 * A soothing pastel color scheme with subdued colors for a muted aesthetic.
 * https://github.com/catppuccin/catppuccin
 */

// Base colors (backgrounds and surfaces)
export const Rosewater = 0xf2d5cf;
export const Flamingo = 0xeebebe;
export const Pink = 0xf4b8e4;
export const Mauve = 0xca9ee6;
export const Red = 0xe78284;
export const Maroon = 0xea999c;
export const Peach = 0xef9f76;
export const Yellow = 0xe5c890;
export const Green = 0xa6d189;
export const Teal = 0x81c8be;
export const Sky = 0x99d1db;
export const Sapphire = 0x85c1dc;
export const Blue = 0x8caaee;
export const Lavender = 0xbabbf1;

// Text colors
export const Text = 0xc6d0f5;
export const Subtext1 = 0xb5bfe2;
export const Subtext0 = 0xa5adce;

// Overlay colors
export const Overlay2 = 0x949cbb;
export const Overlay1 = 0x838ba7;
export const Overlay0 = 0x737994;

// Surface colors
export const Surface2 = 0x626880;
export const Surface1 = 0x51576d;
export const Surface0 = 0x414559;

// Background colors
export const Base = 0x303446;
export const Mantle = 0x292c3c;
export const Crust = 0x232634;

/**
 * Semantic color mappings for UI elements
 */
export const FrappeTheme = {
    // Backgrounds
    background: {
        primary: Base,
        secondary: Mantle,
        tertiary: Crust,
        surface: Surface0,
        surface1: Surface1,
        surface2: Surface2,
    },

    // Text
    text: {
        primary: Text,
        secondary: Subtext1,
        tertiary: Subtext0,
        muted: Overlay2,
    },

    // Overlays and borders
    overlay: {
        light: Overlay0,
        medium: Overlay1,
        dark: Overlay2,
    },

    // Accent colors
    accent: {
        rosewater: Rosewater,
        flamingo: Flamingo,
        pink: Pink,
        mauve: Mauve,
        red: Red,
        maroon: Maroon,
        peach: Peach,
        yellow: Yellow,
        green: Green,
        teal: Teal,
        sky: Sky,
        sapphire: Sapphire,
        blue: Blue,
        lavender: Lavender,
    },

    // Semantic UI colors
    ui: {
        // Health and status
        health: Red,
        healthBar: Red,
        healthRegen: Maroon,

        // Energy and shields
        energy: Blue,
        energyBar: Blue,
        shield: Sapphire,
        shieldBar: Sky,

        // Experience and progression
        experience: Mauve,
        experienceBar: Lavender,

        // Success/error states
        success: Green,
        warning: Yellow,
        error: Red,
        info: Blue,

        // Interactive elements
        button: Surface1,
        buttonHover: Surface2,
        buttonActive: Overlay0,
        input: Surface0,
        inputFocus: Surface1,
        border: Overlay0,
        borderFocus: Overlay1,

        // Selection and hover
        selection: Surface2,
        hover: Overlay0,

        // Special effects
        particle: Teal,
        impact: Peach,
        death: Red,

        // Weapons and combat
        projectile: Yellow,
        damage: Red,

        // Items and pickups
        pickup: Green,
        rare: Pink,
        epic: Mauve,
        legendary: Peach,
    },

    // Game entity colors
    entity: {
        player: Teal,
        enemy: Red,
        neutral: Overlay1,      // Gray for neutral hulls
        ally: Green,
        asteroid: Overlay1,
        background: Crust,
        hull: Overlay1,         // Ship hull parts
        shield: Sapphire,       // Shield components
        engine: Peach,          // Engine components
        nozzle: Peach,          // Engine nozzles
    },

    // UI panels and HUD
    hud: {
        panel: Base,
        panelBorder: Surface2,
        header: Mantle,
        divider: Surface1,
        shadow: Crust,
    },

    // Chat and communication
    chat: {
        background: Base,
        border: Surface1,
        message: Text,
        timestamp: Subtext0,
        system: Lavender,
        error: Red,
        username: Mauve,
    },

    // Performance and debug
    debug: {
        fps: Green,
        warning: Yellow,
        error: Red,
        info: Blue,
    },
};

/**
 * Convert hex color to RGB components
 */
export function hexToRgb(hex: number): { r: number; g: number; b: number } {
    return {
        r: (hex >> 16) & 0xff,
        g: (hex >> 8) & 0xff,
        b: hex & 0xff,
    };
}

/**
 * Convert RGB components to hex color
 */
export function rgbToHex(r: number, g: number, b: number): number {
    return (r << 16) | (g << 8) | b;
}

/**
 * Get color with alpha as CSS string
 */
export function colorWithAlpha(hex: number, alpha: number): string {
    const rgb = hexToRgb(hex);
    return `rgba(${rgb.r}, ${rgb.g}, ${rgb.b}, ${alpha})`;
}

/**
 * Convert hex color to CSS hex string
 */
export function hexToCss(hex: number): string {
    return '#' + hex.toString(16).padStart(6, '0');
}

/**
 * Interpolate between two colors
 */
export function interpolateColor(color1: number, color2: number, t: number): number {
    const rgb1 = hexToRgb(color1);
    const rgb2 = hexToRgb(color2);

    const r = Math.round(rgb1.r + (rgb2.r - rgb1.r) * t);
    const g = Math.round(rgb1.g + (rgb2.g - rgb1.g) * t);
    const b = Math.round(rgb1.b + (rgb2.b - rgb1.b) * t);

    return rgbToHex(r, g, b);
}
