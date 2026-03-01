import { TestBed } from '@angular/core/testing';
import { ForkliftParserService } from './forklift-parser.service';

describe('ForkliftParserService', () => {
  let service: ForkliftParserService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ForkliftParserService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should return empty actions and null error for empty command', () => {
    const result = service.parseCommand('');
    expect(result.actions).toEqual([]);
    expect(result.error).toBeNull();
  });

  it('should parse Forward command correctly', () => {
    const result = service.parseCommand('F10');
    expect(result.actions).toEqual(['Move Forward by 10 metres.']);
    expect(result.error).toBeNull();
  });

  it('should parse Backward command correctly', () => {
    const result = service.parseCommand('B5');
    expect(result.actions).toEqual(['Move Backward by 5 metres.']);
    expect(result.error).toBeNull();
  });

  it('should parse Left command correctly', () => {
    const result = service.parseCommand('L90');
    expect(result.actions).toEqual(['Turn Left by 90 degrees.']);
    expect(result.error).toBeNull();
  });

  it('should parse Right command correctly', () => {
    const result = service.parseCommand('R180');
    expect(result.actions).toEqual(['Turn Right by 180 degrees.']);
    expect(result.error).toBeNull();
  });

  it('should parse valid commands correctly', () => {
    const result = service.parseCommand('F10R90B5L180');
    expect(result.error).toBeNull();
    expect(result.actions).toEqual([
      'Move Forward by 10 metres.',
      'Turn Right by 90 degrees.',
      'Move Backward by 5 metres.',
      'Turn Left by 180 degrees.',
    ]);
  });
});
