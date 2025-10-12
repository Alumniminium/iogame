import { System1 } from "../core/System";
import { NTT } from "../core/NTT";
import { LifeTimeComponent } from "../components/LifeTimeComponent";
import { DeathTagComponent } from "../components/DeathTagComponent";

export class LifetimeSystem extends System1<LifeTimeComponent> {
  constructor() {
    super(LifeTimeComponent);
  }

  protected updateEntity(ntt: NTT, ltc: LifeTimeComponent, deltaTime: number): void {
    ltc.lifetimeSeconds -= deltaTime;

    if (ltc.lifetimeSeconds > 0)
      return

    ntt.set(new DeathTagComponent(ntt));
  }
}
