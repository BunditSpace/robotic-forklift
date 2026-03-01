import { Injectable } from '@angular/core';
import { ParseResult } from '../shared/models/parseResult';

@Injectable({ providedIn: 'root' })
export class ForkliftParserService {
  /**
   * Parses a forklift command string and returns the corresponding actions or an error message if the command is invalid.
   * @param commandString The command string to parse.
   * @returns A ParseResult object containing the parsed actions and any error message.
   */
  parseCommand(commandString: string): ParseResult {
    const actions: string[] = [];

    if (!commandString.trim()) {
      return { actions, error: null };
    }

    const str = commandString.trim().toUpperCase();

    const structureError = this.validateStructure(str);
    if (structureError) return { actions: [], error: structureError };

    const tokenRegex = /([FBLR])(\d+)/g;
    let match: RegExpExecArray | null;

    while ((match = tokenRegex.exec(str)) !== null) {
      const [, char, numStr] = match;
      const num = parseInt(numStr, 10);

      const rotationError = this.validateRotation(char, num);
      if (rotationError) return { actions: [], error: rotationError };

      if (char === 'L' || char === 'R') {
        actions.push(`Turn ${char === 'L' ? 'Left' : 'Right'} by ${num} degrees.`);
      } else {
        actions.push(`Move ${char === 'F' ? 'Forward' : 'Backward'} by ${num} metres.`);
      }
    }

    return { actions, error: null };
  }

  /**
   * Validates the structure of a command string.
   * @param str The command string to validate.
   * @returns A string containing the error message if the structure is invalid, or null if the structure is valid.
   */
  private validateStructure(str: string): string | null {
    const invalidChar = str.match(/[^FBLR0-9]/);
    if (invalidChar) {
      const pos = str.indexOf(invalidChar[0]) + 1;
      return `Invalid command at position ${pos}: Expected F, B, L, or R but found '${invalidChar[0]}'.`;
    }

    if (/^[0-9]/.test(str)) {
      return `Invalid command at position 1: Expected F, B, L, or R but found '${str[0]}'.`;
    }

    const missingNumber = str.match(/[FBLR](?![0-9])/);
    if (missingNumber) {
      const pos = str.indexOf(missingNumber[0]) + 1;
      return `Invalid command at position ${pos}: Missing number after '${missingNumber[0]}'.`;
    }

    return null;
  }

  /**
   * Validates the rotation command for 'L' and 'R'.
   * @param char The command character ('L' or 'R').
   * @param num  The number of degrees to rotate.
   * @returns A string containing the error message if the rotation is invalid, or null if the rotation is valid.
   */
  private validateRotation(char: string, num: number): string | null {
    if (char !== 'L' && char !== 'R') return null;

    if (num < 0 || num > 360) {
      return `Invalid degrees for ${char}: Must be between 0 and 360 (found ${num}).`;
    }

    if (num % 90 !== 0) {
      return `Invalid degrees for ${char}: Must be a multiple of 90 (e.g., 90, 180, 270).`;
    }

    return null;
  }
}
