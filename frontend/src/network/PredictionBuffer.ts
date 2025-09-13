export interface InputCommand {
  sequenceNumber: number;
  timestamp: number;
  moveX: number;
  moveY: number;
  mouseX: number;
  mouseY: number;
  buttons: number;
}

export interface PredictionState {
  sequenceNumber: number;
  position: { x: number; y: number };
  velocity: { x: number; y: number };
  rotation: number;
}

export class PredictionBuffer {
  private unacknowledgedInputs: InputCommand[] = [];
  private predictionStates: Map<number, PredictionState> = new Map();
  private currentSequence = 0;
  private maxBufferSize = 120;
  
  addInput(input: Omit<InputCommand, 'sequenceNumber'>): InputCommand {
    const command: InputCommand = {
      ...input,
      sequenceNumber: this.currentSequence++
    };
    
    this.unacknowledgedInputs.push(command);
    
    // Limit buffer size
    while (this.unacknowledgedInputs.length > this.maxBufferSize) {
      const removed = this.unacknowledgedInputs.shift();
      if (removed) {
        this.predictionStates.delete(removed.sequenceNumber);
      }
    }
    
    return command;
  }
  
  savePredictionState(sequenceNumber: number, state: Omit<PredictionState, 'sequenceNumber'>): void {
    this.predictionStates.set(sequenceNumber, {
      ...state,
      sequenceNumber
    });
  }
  
  reconcile(acknowledgedSequence: number, serverState: { 
    position: { x: number; y: number };
    velocity: { x: number; y: number };
    rotation: number;
  }): InputCommand[] {
    // Remove acknowledged inputs
    this.unacknowledgedInputs = this.unacknowledgedInputs.filter(
      input => input.sequenceNumber > acknowledgedSequence
    );
    
    // Clean up old prediction states
    this.predictionStates.forEach((_, seq) => {
      if (seq <= acknowledgedSequence) {
        this.predictionStates.delete(seq);
      }
    });
    
    // Check if we need to reconcile
    const predictedState = this.predictionStates.get(acknowledgedSequence);
    if (predictedState) {
      const positionError = Math.sqrt(
        Math.pow(predictedState.position.x - serverState.position.x, 2) +
        Math.pow(predictedState.position.y - serverState.position.y, 2)
      );
      
      // If error is significant, we need to replay inputs
      if (positionError > 0.1) {
        return this.unacknowledgedInputs;
      }
    }
    
    return [];
  }
  
  getUnacknowledgedInputs(): InputCommand[] {
    return this.unacknowledgedInputs;
  }
  
  getCurrentSequence(): number {
    return this.currentSequence;
  }
  
  clear(): void {
    this.unacknowledgedInputs = [];
    this.predictionStates.clear();
    this.currentSequence = 0;
  }
}